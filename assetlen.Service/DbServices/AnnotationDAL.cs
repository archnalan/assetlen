using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IAnnotationDAL"/>
public class AnnotationDAL : IAnnotationDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<AnnotationDAL> _logger;
    private readonly IProjectAccessService _access;
    private readonly IArtifactDAL _artifacts;
    private readonly ICommitmentDAL _commitments;

    private const int MaxShapes = 40;
    private const int MaxPathNumbers = 2000;

    public AnnotationDAL(AssetlenDbContext context, ILogger<AnnotationDAL> logger,
        IProjectAccessService access, IArtifactDAL artifacts, ICommitmentDAL commitments)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _artifacts = artifacts;
        _commitments = commitments;
    }

    // ─── Read ────────────────────────────────────────────────────────────

    public async Task<ServiceResult<AnnotatedArtifactDto>> GetForArtifact(string artifactId, string userId, CancellationToken ct = default)
    {
        try
        {
            var (artifact, access, error) = await LoadArtifactAsync(artifactId, userId, ct);
            if (error is not null) return Fail<AnnotatedArtifactDto>(error);

            var rows = await _context.tbl_Annotations.AsNoTracking()
                .Include(a => a.Author)
                .Where(a => a.ArtifactId == artifactId)
                .OrderBy(a => a.DateTimeCreated)
                .ToListAsync(ct);

            var visible = rows.Where(r => CanSee(r, access)).ToList();
            var dtos = await ToDtosAsync(visible, rows, artifact!.ProjectId!, access, userId, ct);

            var linked = new List<CommitmentLinkDto>();
            if (access.CanSeeRegister)
            {
                var back = await _commitments.GetBacklinks(artifact.ProjectId!, CommitmentLinkTarget.Artifact, artifactId, userId);
                if (back.IsSuccess) linked = back.Data;
            }

            return ServiceResult<AnnotatedArtifactDto>.Success(new AnnotatedArtifactDto
            {
                Artifact = artifact,
                Layers = dtos.Where(d => d.IsCurrent).ToList(),
                History = dtos.Where(d => !d.IsCurrent).OrderByDescending(d => d.DateTimeCreated).ToList(),
                CanAnnotate = access.CanWrite,
                CanAsk = access.CanWrite && access.CanSeeRegister,
                LinkedCommitments = linked
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading annotations on {ArtifactId}", artifactId);
            return Fail<AnnotatedArtifactDto>(new ServerErrorException(ex.Message));
        }
    }

    // ─── Write ───────────────────────────────────────────────────────────

    public async Task<ServiceResult<AnnotationDto>> Save(AnnotationSaveDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (artifact, access, error) = await LoadArtifactAsync(dto.ArtifactId, userId, ct);
            if (error is not null) return Fail<AnnotationDto>(error);
            if (!access.CanWrite) return Fail<AnnotationDto>(new ForbiddenException("You can look at this file but not mark it up."));

            var shapes = Clean(dto.Shapes, out var shapeError);
            if (shapeError is not null) return Fail<AnnotationDto>(new BadRequestException(shapeError));

            var now = DateTime.UtcNow;
            tbl_Annotation row;

            if (string.IsNullOrEmpty(dto.LayerId))
            {
                if (shapes.Count == 0) return Fail<AnnotationDto>(new BadRequestException("Draw something first."));
                var id = Guid.NewGuid().ToString();
                row = new tbl_Annotation
                {
                    Id = id,
                    LayerId = id,
                    Version = 1,
                    // Fail-closed (CLAUDE.md §5.4): the delivery side's marks are its
                    // own until the mediator exposes them. The client side has only
                    // one channel to draw on.
                    Channel = access.Side == ProjectSide.Client ? Channel.Client : Channel.Crew
                };
            }
            else
            {
                var current = await _context.tbl_Annotations
                    .FirstOrDefaultAsync(a => a.LayerId == dto.LayerId && a.ArtifactId == dto.ArtifactId && a.SupersededAt == null, ct);
                if (current is null || !CanSee(current, access))
                    return Fail<AnnotationDto>(new NotFoundException("Layer not found."));

                // A layer is attributed, so only its author adds to it. Anyone
                // else who disagrees draws their own layer beside it.
                if (current.AuthorId != userId)
                    return Fail<AnnotationDto>(new ForbiddenException("This layer is someone else's. Draw your own beside it."));

                current.SupersededAt = now;
                var keepsExposure = current.Channel == Channel.Client
                                    && (access.Side == ProjectSide.Client || access.CanExposeToClient || current.CommitmentId is not null);
                row = new tbl_Annotation
                {
                    Id = Guid.NewGuid().ToString(),
                    LayerId = current.LayerId,
                    Version = current.Version + 1,
                    CommitmentId = current.CommitmentId,
                    Channel = keepsExposure ? Channel.Client : Channel.Crew
                };
            }

            row.ProjectId = artifact!.ProjectId;
            row.ArtifactId = artifact.Id;
            row.AuthorId = userId;
            row.AuthorSide = access.Side;
            row.ShapesJson = JsonSerializer.Serialize(shapes);
            row.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

            _context.tbl_Annotations.Add(row);
            await _context.SaveChangesAsync(ct);

            return await OneAsync(row.Id, access, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving a layer on {ArtifactId}", dto.ArtifactId);
            return Fail<AnnotationDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<AskResultDto>> Ask(AnnotationAskDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var (artifact, access, error) = await LoadArtifactAsync(dto.ArtifactId, userId, ct);
            if (error is not null) return Fail<AskResultDto>(error);
            if (!access.CanSeeRegister) return Fail<AskResultDto>(new NotFoundException("Commitment not found."));
            if (!access.CanWrite) return Fail<AskResultDto>(new ForbiddenException("You can read the register but not query it."));
            if (string.IsNullOrWhiteSpace(dto.Question)) return Fail<AskResultDto>(new BadRequestException("Say what the question is."));

            var shapes = Clean(dto.Shapes, out var shapeError);
            if (shapeError is not null) return Fail<AskResultDto>(new BadRequestException(shapeError));
            if (shapes.Count == 0) return Fail<AskResultDto>(new BadRequestException("Circle what you are asking about."));

            var target = await _commitments.GetCommitment(dto.CommitmentId!, userId);
            if (!target.IsSuccess) return Fail<AskResultDto>(target.Error);
            if (target.Data.ProjectId != artifact!.ProjectId)
                return Fail<AskResultDto>(new BadRequestException("That commitment is on a different project from this file."));

            // The query itself is the register's: the same rules, the same flag,
            // owed to the same person — whether it was typed or circled.
            var raised = await _commitments.RaiseQuery(new CommitmentNoteDto { CommitmentId = target.Data.Id, Note = dto.Question.Trim() }, userId);
            if (!raised.IsSuccess) return Fail<AskResultDto>(raised.Error);

            var id = Guid.NewGuid().ToString();
            var row = new tbl_Annotation
            {
                Id = id,
                LayerId = id,
                Version = 1,
                ProjectId = artifact.ProjectId,
                ArtifactId = artifact.Id,
                AuthorId = userId,
                AuthorSide = access.Side,
                // A query on a commitment is between the two principals, so it crosses by definition.
                Channel = Channel.Client,
                ShapesJson = JsonSerializer.Serialize(shapes),
                Note = dto.Question.Trim(),
                CommitmentId = target.Data.Id
            };
            _context.tbl_Annotations.Add(row);
            await _context.SaveChangesAsync(ct);

            await _commitments.AddLink(new CommitmentLinkCreateDto
            {
                CommitmentId = target.Data.Id,
                TargetType = CommitmentLinkTarget.Annotation,
                TargetId = row.LayerId,
                Relation = CommitmentLinkRelation.Relates,
                Note = "Asked on this markup"
            }, userId);

            var alreadyLinked = await _context.tbl_CommitmentLinks.AnyAsync(l => l.CommitmentId == target.Data.Id
                && l.TargetType == CommitmentLinkTarget.Artifact && l.TargetId == artifact.Id, ct);
            if (!alreadyLinked)
            {
                await _commitments.AddLink(new CommitmentLinkCreateDto
                {
                    CommitmentId = target.Data.Id,
                    TargetType = CommitmentLinkTarget.Artifact,
                    TargetId = artifact.Id,
                    Relation = CommitmentLinkRelation.Relates,
                    Note = "Questioned"
                }, userId);
            }

            var annotation = await OneAsync(row.Id, access, userId, ct);
            var commitment = await _commitments.GetCommitment(target.Data.Id!, userId);
            return ServiceResult<AskResultDto>.Success(new AskResultDto
            {
                Annotation = annotation.IsSuccess ? annotation.Data : null,
                Commitment = commitment.IsSuccess ? commitment.Data : raised.Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asking on {ArtifactId}", dto.ArtifactId);
            return Fail<AskResultDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<AnnotationDto>> Expose(AnnotationExposeDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var row = await _context.tbl_Annotations
                .FirstOrDefaultAsync(a => a.LayerId == dto.LayerId && a.SupersededAt == null, ct);
            if (row is null) return Fail<AnnotationDto>(new NotFoundException("Layer not found."));

            var (_, access, error) = await LoadArtifactAsync(row.ArtifactId, userId, ct);
            if (error is not null || !CanSee(row, access)) return Fail<AnnotationDto>(new NotFoundException("Layer not found."));
            if (!access.CanExposeToClient)
                return Fail<AnnotationDto>(new ForbiddenException("Only the mediator moves marks across to the client side."));
            if (!Enum.IsDefined(dto.Channel)) return Fail<AnnotationDto>(new BadRequestException("Unknown channel."));
            if (dto.Channel == Channel.Crew && (row.AuthorSide == ProjectSide.Client || row.CommitmentId is not null))
                return Fail<AnnotationDto>(new BadRequestException("This layer belongs to the client side's record and stays there."));

            // Exposure is a pointer decision, like an artifact ref's channel —
            // it moves the layer, it does not change what was drawn.
            row.Channel = dto.Channel;
            await _context.SaveChangesAsync(ct);
            return await OneAsync(row.Id, access, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exposing layer {LayerId}", dto.LayerId);
            return Fail<AnnotationDto>(new ServerErrorException(ex.Message));
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ServiceResult<T> Fail<T>(Exception ex) => ServiceResult<T>.Failure(ex);

    /// <summary>The file decides first: a layer is never a way to learn that a file exists.</summary>
    private async Task<(ArtifactDto? Artifact, ProjectAccess Access, Exception? Error)> LoadArtifactAsync(
        string? artifactId, string userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(artifactId))
            return (null, ProjectAccess.None, new BadRequestException("ArtifactId is required."));

        var artifact = await _artifacts.GetAsync(artifactId, userId, ct);
        if (!artifact.IsSuccess || artifact.Data?.ProjectId is null)
            return (null, ProjectAccess.None, new NotFoundException("Artifact not found."));

        var access = await _access.ResolveAsync(artifact.Data.ProjectId, userId, ct);
        return (artifact.Data, access, null);
    }

    private static bool CanSee(tbl_Annotation a, ProjectAccess access) =>
        (a.Channel == Channel.Client || access.CanSeeSiteLog)
        && (a.CommitmentId is null || access.CanSeeRegister);

    private async Task<ServiceResult<AnnotationDto>> OneAsync(string rowId, ProjectAccess access, string userId, CancellationToken ct)
    {
        var row = await _context.tbl_Annotations.AsNoTracking().Include(a => a.Author).FirstAsync(a => a.Id == rowId, ct);
        var layer = await _context.tbl_Annotations.AsNoTracking().Where(a => a.LayerId == row.LayerId).ToListAsync(ct);
        var dtos = await ToDtosAsync(new List<tbl_Annotation> { row }, layer, row.ProjectId!, access, userId, ct);
        return ServiceResult<AnnotationDto>.Success(dtos[0]);
    }

    private async Task<List<AnnotationDto>> ToDtosAsync(
        List<tbl_Annotation> shown, List<tbl_Annotation> all, string projectId, ProjectAccess access, string userId, CancellationToken ct)
    {
        // One accountable face (§10.1): the client side reads the mediator's
        // name on anything the delivery side drew, never the bench's.
        string? face = null;
        if (!access.CanSeeSiteLog && shown.Any(a => a.AuthorSide == ProjectSide.Contractor))
        {
            var parent = await _context.tbl_Projects_RS.AsNoTracking()
                .Where(p => p.Id == projectId).Select(p => p.ParentProjectId).FirstOrDefaultAsync(ct);
            var mediator = await _context.tbl_ProjectMembers.AsNoTracking()
                .Include(m => m.User)
                .Where(m => (m.ProjectId == projectId || m.ProjectId == parent) && m.IsActive && m.IsMediator)
                .OrderBy(m => m.ProjectId == projectId ? 0 : 1).ThenBy(m => m.JoinedAt ?? m.DateTimeCreated)
                .FirstOrDefaultAsync(ct);
            face = mediator is null ? "The delivery side"
                : (mediator.User is { } u ? $"{u.FirstName} {u.LastName}".Trim() : mediator.PartyName ?? mediator.Title ?? "The mediator");
        }

        var commitmentIds = shown.Select(a => a.CommitmentId).OfType<string>().Distinct().ToList();
        var commitments = commitmentIds.Count == 0
            ? new Dictionary<string, tbl_Commitment>()
            : await _context.tbl_Commitments.AsNoTracking()
                .Where(c => c.ProjectId == projectId)
                .ToDictionaryAsync(c => c.Id, ct);

        string? Head(string? id)
        {
            var guard = 0;
            while (id is not null && commitments.TryGetValue(id, out var c) && c.SupersededById is { } next && guard++ < 500) id = next;
            return id;
        }

        return shown.Select(a =>
        {
            var author = a.Author is null ? null : $"{a.Author.FirstName} {a.Author.LastName}".Trim();
            var head = Head(a.CommitmentId);
            var headRow = head is not null && commitments.TryGetValue(head, out var h) ? h : null;
            return new AnnotationDto
            {
                Id = a.Id,
                LayerId = a.LayerId,
                Version = a.Version,
                VersionCount = all.Count(x => x.LayerId == a.LayerId),
                IsCurrent = a.SupersededAt is null,
                ArtifactId = a.ArtifactId,
                ProjectId = a.ProjectId,
                AuthorName = !access.CanSeeSiteLog && a.AuthorSide == ProjectSide.Contractor ? face : author,
                AuthorSide = a.AuthorSide,
                IsMine = a.AuthorId == userId,
                Channel = a.Channel,
                Shapes = Parse(a.ShapesJson),
                Note = a.Note,
                CommitmentId = a.CommitmentId,
                CommitmentTitle = a.CommitmentId is not null && commitments.TryGetValue(a.CommitmentId, out var c) ? c.Title : null,
                CommitmentQueryState = headRow?.QueryState,
                CurrentCommitmentId = head,
                CanEdit = a.SupersededAt is null && a.AuthorId == userId && access.CanWrite,
                CanExpose = a.SupersededAt is null && access.CanExposeToClient
                            && a.AuthorSide != ProjectSide.Client && a.CommitmentId is null,
                DateTimeCreated = a.DateTimeCreated
            };
        }).ToList();
    }

    private static List<AnnotationShapeDto> Parse(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<AnnotationShapeDto>>(json) ?? new(); }
        catch (JsonException) { return new(); }
    }

    /// <summary>Marks are fractions of the image. Anything outside it, or not a mark we draw, is refused rather than stored.</summary>
    private static List<AnnotationShapeDto> Clean(List<AnnotationShapeDto>? shapes, out string? error)
    {
        error = null;
        var result = new List<AnnotationShapeDto>();
        if (shapes is null) return result;
        if (shapes.Count > MaxShapes) { error = $"At most {MaxShapes} marks on one layer."; return result; }

        static bool In(double v) => double.IsFinite(v) && v >= -0.0001 && v <= 1.0001;
        static double R(double v) => Math.Round(Math.Clamp(v, 0, 1), 4);

        foreach (var s in shapes)
        {
            if (!Enum.IsDefined(s.Kind)) { error = "Unknown mark."; return result; }
            switch (s.Kind)
            {
                case AnnotationShapeKind.Ellipse or AnnotationShapeKind.Rect:
                    if (!In(s.X) || !In(s.Y) || !In(s.X + s.W) || !In(s.Y + s.H) || s.W <= 0.002 || s.H <= 0.002)
                    { error = "A mark falls outside the file."; return result; }
                    result.Add(new AnnotationShapeDto { Kind = s.Kind, X = R(s.X), Y = R(s.Y), W = Math.Round(s.W, 4), H = Math.Round(s.H, 4) });
                    break;
                case AnnotationShapeKind.Arrow:
                    if (!In(s.X) || !In(s.Y) || !In(s.X + s.W) || !In(s.Y + s.H) || (Math.Abs(s.W) < 0.002 && Math.Abs(s.H) < 0.002))
                    { error = "A mark falls outside the file."; return result; }
                    result.Add(new AnnotationShapeDto { Kind = s.Kind, X = R(s.X), Y = R(s.Y), W = Math.Round(s.W, 4), H = Math.Round(s.H, 4) });
                    break;
                case AnnotationShapeKind.Path:
                    var pts = s.Points ?? new();
                    if (pts.Count < 4 || pts.Count % 2 != 0 || pts.Count > MaxPathNumbers || pts.Any(p => !In(p)))
                    { error = "A drawn line falls outside the file."; return result; }
                    result.Add(new AnnotationShapeDto { Kind = s.Kind, Points = pts.Select(R).ToList() });
                    break;
            }
        }
        return result;
    }
}
