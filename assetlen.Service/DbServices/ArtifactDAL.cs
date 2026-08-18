using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IArtifactDAL"/>
public class ArtifactDAL : IArtifactDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<ArtifactDAL> _logger;
    private readonly IArtifactStorage _storage;
    private readonly IThumbnailGenerator _thumbnails;
    private readonly IProjectAccessService _access;

    private const int ThumbnailMaxEdge = 480;
    private const long MaxUploadBytes = 40L * 1024 * 1024;

    public ArtifactDAL(
        AssetlenDbContext context,
        ILogger<ArtifactDAL> logger,
        IArtifactStorage storage,
        IThumbnailGenerator thumbnails,
        IProjectAccessService access)
    {
        _context = context;
        _logger = logger;
        _storage = storage;
        _thumbnails = thumbnails;
        _access = access;
    }

    // ─── Ingest ──────────────────────────────────────────────────────────

    public async Task<ServiceResult<ArtifactDto>> IngestAsync(
        Stream content, string? fileName, string? contentType, string projectId, string userId,
        DateTime? capturedAt = null, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
                return Fail<ArtifactDto>(new BadRequestException("ProjectId is required."));

            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null)
                return Fail<ArtifactDto>(new NotFoundException("Project not found."));

            // Anyone who may contribute to the project may upload. Uploading is
            // not exposing — everything lands Crew-only regardless of who sends it.
            if (!await _access.CanWriteAsync(project, userId, ct))
                return Fail<ArtifactDto>(new ForbiddenException("Access denied."));

            // Buffer once: we need the bytes twice (hash, then store) and the
            // request stream is forward-only.
            await using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, ct);
            if (buffer.Length == 0)
                return Fail<ArtifactDto>(new BadRequestException("File is empty."));
            if (buffer.Length > MaxUploadBytes)
                return Fail<ArtifactDto>(new BadRequestException($"File exceeds the {MaxUploadBytes / (1024 * 1024)} MB limit."));

            buffer.Position = 0;
            var sha256 = Convert.ToHexString(await SHA256.HashDataAsync(buffer, ct)).ToLowerInvariant();

            // Law 2: the same bytes are the same artifact. A re-upload returns
            // what already exists rather than creating a second copy.
            var existing = await _context.tbl_Artifacts
                .Include(a => a.UploadedBy)
                .FirstOrDefaultAsync(a => a.Sha256 == sha256 && a.ProjectId == projectId, ct);

            if (existing is not null)
            {
                var refCount = await _context.tbl_ArtifactRefs.CountAsync(r => r.ArtifactId == existing.Id, ct);
                var dto = ToDto(existing, refCount);
                dto.WasDeduplicated = true;
                return ServiceResult<ArtifactDto>.Success(dto);
            }

            var mime = NormalizeMime(contentType, fileName);
            var extension = ExtensionFor(mime, fileName);

            buffer.Position = 0;
            var storagePath = await _storage.PutAsync(sha256, extension, buffer, ct);

            string? thumbnailPath = null;
            int? width = null, height = null;

            if (_thumbnails.CanHandle(mime))
            {
                buffer.Position = 0;
                var generated = await _thumbnails.CreateAsync(buffer, ThumbnailMaxEdge, ct);
                if (generated is { } result)
                {
                    await using var thumb = result.Thumbnail;
                    thumbnailPath = await _storage.PutThumbnailAsync(sha256, ".jpg", thumb, ct);
                    width = result.Source.Width;
                    height = result.Source.Height;
                }
            }

            var artifact = new tbl_Artifact
            {
                ProjectId = projectId,
                Sha256 = sha256,
                ByteSize = buffer.Length,
                MimeType = mime,
                StoragePath = storagePath,
                ThumbnailPath = thumbnailPath,
                OriginalFileName = Truncate(fileName, 260),
                UploadedById = userId,
                CapturedAt = capturedAt ?? DateTime.UtcNow,
                Width = width,
                Height = height
            };

            _context.tbl_Artifacts.Add(artifact);

            var collided = false;
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Two uploads of the same file raced past the lookup above and
                // collided on the unique index. Re-check before swallowing it —
                // a DbUpdateException for any other reason must still surface.
                _context.Entry(artifact).State = EntityState.Detached;
                if (!await HashExistsAsync(sha256, projectId, ct)) throw;
                collided = true;
            }

            if (collided)
            {
                // The bytes are already stored at the same content address, so
                // the loser of the race just adopts the winner.
                var winner = await _context.tbl_Artifacts
                    .Include(a => a.UploadedBy)
                    .FirstAsync(a => a.Sha256 == sha256 && a.ProjectId == projectId, ct);
                var winnerDto = ToDto(winner, 0);
                winnerDto.WasDeduplicated = true;
                return ServiceResult<ArtifactDto>.Success(winnerDto);
            }

            return ServiceResult<ArtifactDto>.Success(ToDto(artifact, 0));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting artifact into project {ProjectId}", projectId);
            return Fail<ArtifactDto>(new ServerErrorException(ex.Message));
        }
    }

    private Task<bool> HashExistsAsync(string sha256, string projectId, CancellationToken ct) =>
        _context.tbl_Artifacts.AnyAsync(a => a.Sha256 == sha256 && a.ProjectId == projectId, ct);

    // ─── Read ────────────────────────────────────────────────────────────

    public async Task<ServiceResult<ArtifactDto>> GetAsync(string artifactId, string userId, CancellationToken ct = default)
    {
        try
        {
            var artifact = await _context.tbl_Artifacts
                .Include(a => a.UploadedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == artifactId, ct);
            if (artifact is null)
                return Fail<ArtifactDto>(new NotFoundException("Artifact not found."));

            var access = await _access.ResolveAsync(artifact.ProjectId, userId, ct);
            if (!access.CanRead)
                return Fail<ArtifactDto>(new NotFoundException("Artifact not found."));

            if (!await IsVisibleAsync(artifactId, access, ct))
                return Fail<ArtifactDto>(new NotFoundException("Artifact not found."));

            var refCount = await _context.tbl_ArtifactRefs.CountAsync(r => r.ArtifactId == artifactId, ct);
            return ServiceResult<ArtifactDto>.Success(ToDto(artifact, refCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading artifact {ArtifactId}", artifactId);
            return Fail<ArtifactDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ArtifactContent>> OpenContentAsync(
        string artifactId, bool thumbnail, string userId, CancellationToken ct = default)
    {
        try
        {
            var artifact = await _context.tbl_Artifacts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == artifactId, ct);
            if (artifact is null)
                return Fail<ArtifactContent>(new NotFoundException("Artifact not found."));

            var access = await _access.ResolveAsync(artifact.ProjectId, userId, ct);
            if (!access.CanRead || !await IsVisibleAsync(artifactId, access, ct))
                return Fail<ArtifactContent>(new NotFoundException("Artifact not found."));

            var path = thumbnail ? artifact.ThumbnailPath ?? artifact.StoragePath : artifact.StoragePath;
            if (string.IsNullOrEmpty(path))
                return Fail<ArtifactContent>(new NotFoundException("Artifact has no stored content."));

            var stream = await _storage.OpenAsync(path, ct);
            if (stream is null)
            {
                _logger.LogError("Artifact {ArtifactId} row exists but bytes are missing at {Path}", artifactId, path);
                return Fail<ArtifactContent>(new NotFoundException("Artifact content is missing."));
            }

            var servingThumb = thumbnail && !string.IsNullOrEmpty(artifact.ThumbnailPath);
            return ServiceResult<ArtifactContent>.Success(new ArtifactContent(
                stream,
                servingThumb ? "image/jpeg" : artifact.MimeType ?? "application/octet-stream",
                artifact.OriginalFileName ?? artifact.Sha256 ?? "artifact",
                servingThumb ? stream.Length : artifact.ByteSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening artifact {ArtifactId}", artifactId);
            return Fail<ArtifactContent>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Can this caller reach the artifact through at least one ref they are
    /// allowed to see? A contractor-side member sees every ref; a client-side
    /// member sees only exposed ones. An artifact with no refs at all is
    /// visible to its uploader's side — it has just been ingested and is about
    /// to be attached.
    /// </summary>
    private async Task<bool> IsVisibleAsync(string artifactId, ProjectAccess access, CancellationToken ct)
    {
        if (access.CanSeeSiteLog)
            return true;

        var exposedByRef = await _context.tbl_ArtifactRefs
            .AnyAsync(r => r.ArtifactId == artifactId && r.Channel == Channel.Client, ct);
        if (exposedByRef)
            return true;

        // A controlled document carries its own channel and does not create a
        // ref row, so a ref-only test refused the bytes for every drawing a
        // client was entitled to: GetDocumentsAsync listed it, and the download
        // then 404'd. Visibility follows the document that revision belongs to.
        return await _context.tbl_ArtifactRevisions
            .Include(r => r.Document)
            .AnyAsync(r => r.ArtifactId == artifactId
                        && r.Document != null
                        && r.Document.Channel == Channel.Client, ct);
    }

    // ─── Refs and exposure ───────────────────────────────────────────────

    public async Task<ServiceResult<ArtifactRefDto>> AddRefAsync(
        ArtifactRefCreateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ArtifactId) || string.IsNullOrEmpty(dto.TargetId))
                return Fail<ArtifactRefDto>(new BadRequestException("ArtifactId and TargetId are required."));

            var artifact = await _context.tbl_Artifacts
                .Include(a => a.UploadedBy)
                .FirstOrDefaultAsync(a => a.Id == dto.ArtifactId, ct);
            if (artifact is null)
                return Fail<ArtifactRefDto>(new NotFoundException("Artifact not found."));

            if (!await _access.CanWriteAsync(artifact.ProjectId, userId, ct))
                return Fail<ArtifactRefDto>(new ForbiddenException("Access denied."));

            // Idempotent: pointing the same artifact at the same target twice is
            // the same pointer. This is what stops a re-send producing a duplicate.
            var existing = await _context.tbl_ArtifactRefs
                .FirstOrDefaultAsync(r => r.ArtifactId == dto.ArtifactId
                                       && r.TargetType == dto.TargetType
                                       && r.TargetId == dto.TargetId, ct);
            if (existing is not null)
                return ServiceResult<ArtifactRefDto>.Success(ToDto(existing, artifact, null));

            var reference = new tbl_ArtifactRef
            {
                ArtifactId = dto.ArtifactId,
                ProjectId = artifact.ProjectId,
                TargetType = dto.TargetType,
                TargetId = dto.TargetId,
                Channel = Channel.Crew,   // fail-closed, always
                Caption = dto.Caption,
                DisplayOrder = dto.DisplayOrder
            };

            _context.tbl_ArtifactRefs.Add(reference);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<ArtifactRefDto>.Success(ToDto(reference, artifact, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding artifact ref");
            return Fail<ArtifactRefDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<ArtifactRefDto>>> GetRefsAsync(
        ArtifactTargetType targetType, string targetId, string userId, CancellationToken ct = default)
    {
        try
        {
            var first = await _context.tbl_ArtifactRefs
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.TargetType == targetType && r.TargetId == targetId, ct);

            if (first is null)
                return ServiceResult<List<ArtifactRefDto>>.Success(new List<ArtifactRefDto>());

            var access = await _access.ResolveAsync(first.ProjectId, userId, ct);
            if (!access.CanRead)
                return Fail<List<ArtifactRefDto>>(new ForbiddenException("Access denied."));

            var query = _context.tbl_ArtifactRefs
                .Include(r => r.Artifact).ThenInclude(a => a!.UploadedBy)
                .Include(r => r.ExposedBy)
                .Where(r => r.TargetType == targetType && r.TargetId == targetId)
                .AsNoTracking();

            // THE exposure filter. A client-side reader sees only what a
            // mediator moved across — never the rest of the batch.
            if (!access.CanSeeSiteLog)
                query = query.Where(r => r.Channel == Channel.Client);

            var refs = await query.OrderBy(r => r.DisplayOrder).ThenBy(r => r.DateTimeCreated).ToListAsync(ct);

            return ServiceResult<List<ArtifactRefDto>>.Success(
                refs.Select(r => ToDto(r, r.Artifact, r.ExposedBy)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing artifact refs for {TargetType}/{TargetId}", targetType, targetId);
            return Fail<List<ArtifactRefDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<ArtifactRefDto>> SetRefChannelAsync(
        ArtifactExposureDto dto, string userId, CancellationToken ct = default)
    {
        var batch = await SetRefChannelBatchAsync(
            new ArtifactExposureBatchDto { RefIds = { dto.RefId ?? string.Empty }, Channel = dto.Channel }, userId, ct);

        if (!batch.IsSuccess)
            return ServiceResult<ArtifactRefDto>.Failure(batch.Error);

        var single = batch.Data!.FirstOrDefault();
        return single is null
            ? Fail<ArtifactRefDto>(new NotFoundException("Reference not found."))
            : ServiceResult<ArtifactRefDto>.Success(single);
    }

    public async Task<ServiceResult<List<ArtifactRefDto>>> SetRefChannelBatchAsync(
        ArtifactExposureBatchDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var ids = dto.RefIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids.Count == 0)
                return Fail<List<ArtifactRefDto>>(new BadRequestException("No references supplied."));

            var refs = await _context.tbl_ArtifactRefs
                .Include(r => r.Artifact).ThenInclude(a => a!.UploadedBy)
                .Where(r => ids.Contains(r.Id))
                .ToListAsync(ct);

            if (refs.Count == 0)
                return Fail<List<ArtifactRefDto>>(new NotFoundException("References not found."));

            // One batch may not straddle projects — that would need a separate
            // authorization decision per project and silently half-apply.
            var projectIds = refs.Select(r => r.ProjectId).Distinct().ToList();
            if (projectIds.Count > 1)
                return Fail<List<ArtifactRefDto>>(new BadRequestException("References span multiple projects."));

            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectIds[0], ct);

            // Only a mediator (or owner/manager authority) moves material across
            // the boundary. Peter sees what the architect exposed — nobody else
            // can put anything in front of him, and the clerk cannot self-publish.
            if (!await _access.CanExposeToClientAsync(project, userId, ct))
                return Fail<List<ArtifactRefDto>>(new ForbiddenException(
                    "Only a project mediator, owner or manager can change what the client sees."));

            var exposing = dto.Channel == Channel.Client;
            foreach (var reference in refs)
            {
                if (reference.Channel == dto.Channel) continue;

                reference.Channel = dto.Channel;
                reference.ExposedById = exposing ? userId : null;
                reference.ExposedAt = exposing ? DateTime.UtcNow : null;
            }

            await _context.SaveChangesAsync(ct);

            return ServiceResult<List<ArtifactRefDto>>.Success(
                refs.Select(r => ToDto(r, r.Artifact, null)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting artifact ref channel");
            return Fail<List<ArtifactRefDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<bool>> RemoveRefAsync(string refId, string userId, CancellationToken ct = default)
    {
        try
        {
            var reference = await _context.tbl_ArtifactRefs.FirstOrDefaultAsync(r => r.Id == refId, ct);
            if (reference is null)
                return Fail<bool>(new NotFoundException("Reference not found."));

            if (!await _access.CanWriteAsync(reference.ProjectId, userId, ct))
                return Fail<bool>(new ForbiddenException("Access denied."));

            // The pointer goes; the artifact stays. Bytes are content-addressed
            // and may be referenced from somewhere else entirely.
            _context.tbl_ArtifactRefs.Remove(reference);
            await _context.SaveChangesAsync(ct);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing artifact ref {RefId}", refId);
            return Fail<bool>(new ServerErrorException(ex.Message));
        }
    }

    // ─── Controlled documents ────────────────────────────────────────────

    public async Task<ServiceResult<DocumentDto>> CreateDocumentAsync(
        DocumentCreateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId) || string.IsNullOrWhiteSpace(dto.Title))
                return Fail<DocumentDto>(new BadRequestException("ProjectId and Title are required."));

            if (!await _access.CanWriteAsync(dto.ProjectId, userId, ct))
                return Fail<DocumentDto>(new ForbiddenException("Access denied."));

            var document = new tbl_Document
            {
                ProjectId = dto.ProjectId,
                Kind = dto.Kind,
                Title = dto.Title.Trim(),
                Channel = dto.Channel
            };

            _context.tbl_Documents.Add(document);
            await _context.SaveChangesAsync(ct);

            return ServiceResult<DocumentDto>.Success(ToDto(document, new List<tbl_ArtifactRevision>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return Fail<DocumentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DocumentDto>> AddRevisionAsync(
        ArtifactRevisionCreateDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.DocumentId) || string.IsNullOrEmpty(dto.ArtifactId))
                return Fail<DocumentDto>(new BadRequestException("DocumentId and ArtifactId are required."));

            var document = await _context.tbl_Documents
                .Include(d => d.Revisions)
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentId, ct);
            if (document is null)
                return Fail<DocumentDto>(new NotFoundException("Document not found."));

            if (!await _access.CanWriteAsync(document.ProjectId, userId, ct))
                return Fail<DocumentDto>(new ForbiddenException("Access denied."));

            var artifact = await _context.tbl_Artifacts.FirstOrDefaultAsync(a => a.Id == dto.ArtifactId, ct);
            if (artifact is null || artifact.ProjectId != document.ProjectId)
                return Fail<DocumentDto>(new BadRequestException("Artifact does not belong to this project."));

            var previous = document.Revisions
                .Where(r => r.SupersededByRevisionId == null)
                .OrderByDescending(r => r.RevisionNo)
                .FirstOrDefault();

            var revision = new tbl_ArtifactRevision
            {
                DocumentId = document.Id,
                ArtifactId = dto.ArtifactId,
                RevisionNo = document.Revisions.Count == 0 ? 1 : document.Revisions.Max(r => r.RevisionNo) + 1,
                IssuedById = userId,
                IssuedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.tbl_ArtifactRevisions.Add(revision);
            await _context.SaveChangesAsync(ct);

            // Archive, never delete: "which drawing was current when we poured
            // that slab" has to stay answerable months later.
            if (previous is not null)
                previous.SupersededByRevisionId = revision.Id;

            document.CurrentRevisionId = revision.Id;
            await _context.SaveChangesAsync(ct);

            return await GetDocumentAsync(document.Id, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding revision to document {DocumentId}", dto.DocumentId);
            return Fail<DocumentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<List<DocumentDto>>> GetDocumentsAsync(
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanRead)
                return Fail<List<DocumentDto>>(new ForbiddenException("Access denied."));

            var query = _context.tbl_Documents
                .Include(d => d.Revisions).ThenInclude(r => r.Artifact).ThenInclude(a => a!.UploadedBy)
                .Include(d => d.Revisions).ThenInclude(r => r.IssuedBy)
                .Where(d => d.ProjectId == projectId)
                .AsNoTracking();

            if (!access.CanSeeSiteLog)
                query = query.Where(d => d.Channel == Channel.Client);

            var documents = await query.OrderBy(d => d.Kind).ThenBy(d => d.Title).ToListAsync(ct);

            return ServiceResult<List<DocumentDto>>.Success(
                documents.Select(d => ToDto(d, d.Revisions)).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing documents for project {ProjectId}", projectId);
            return Fail<List<DocumentDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DocumentDto>> GetDocumentAsync(
        string documentId, string userId, CancellationToken ct = default)
    {
        try
        {
            var document = await _context.tbl_Documents
                .Include(d => d.Revisions).ThenInclude(r => r.Artifact).ThenInclude(a => a!.UploadedBy)
                .Include(d => d.Revisions).ThenInclude(r => r.IssuedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == documentId, ct);
            if (document is null)
                return Fail<DocumentDto>(new NotFoundException("Document not found."));

            var access = await _access.ResolveAsync(document.ProjectId, userId, ct);
            if (!access.CanRead)
                return Fail<DocumentDto>(new NotFoundException("Document not found."));
            if (!access.CanSeeSiteLog && document.Channel != Channel.Client)
                return Fail<DocumentDto>(new NotFoundException("Document not found."));

            return ServiceResult<DocumentDto>.Success(ToDto(document, document.Revisions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading document {DocumentId}", documentId);
            return Fail<DocumentDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<DocumentDto>> SetDocumentChannelAsync(
        string documentId, Channel channel, string userId, CancellationToken ct = default)
    {
        try
        {
            var document = await _context.tbl_Documents
                .Include(d => d.Project)
                    .ThenInclude(p => p!.ParentProject)
                .FirstOrDefaultAsync(d => d.Id == documentId, ct);
            if (document is null)
                return Fail<DocumentDto>(new NotFoundException("Document not found."));

            // Same gate as a Site Diary frame. Issuing a drawing to the client is
            // the act that makes it the drawing they build expectations on.
            if (!await _access.CanExposeToClientAsync(document.Project, userId, ct))
                return Fail<DocumentDto>(new ForbiddenException(
                    "Only a project mediator, owner or manager can change what the client sees."));

            document.Channel = channel;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Document {DocumentId} set to {Channel} by {UserId}", documentId, channel, userId);

            return await GetDocumentAsync(documentId, userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting channel on document {DocumentId}", documentId);
            return Fail<DocumentDto>(new ServerErrorException(ex.Message));
        }
    }

    // ─── Mapping helpers ─────────────────────────────────────────────────

    private static ServiceResult<T> Fail<T>(Exception ex) => ServiceResult<T>.Failure(ex);

    internal static ArtifactDto ToDto(tbl_Artifact a, int referenceCount) => new()
    {
        Id = a.Id,
        ProjectId = a.ProjectId,
        Sha256 = a.Sha256,
        ByteSize = a.ByteSize,
        MimeType = a.MimeType,
        OriginalFileName = a.OriginalFileName,
        UploadedById = a.UploadedById,
        UploadedByName = a.UploadedBy is not null
            ? $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}".Trim() : null,
        CapturedAt = a.CapturedAt,
        Width = a.Width,
        Height = a.Height,
        DateTimeCreated = a.DateTimeCreated,
        Url = $"/api/Artifacts/{a.Id}/content",
        ThumbnailUrl = a.ThumbnailPath is null ? null : $"/api/Artifacts/{a.Id}/thumbnail",
        ReferenceCount = referenceCount
    };

    private static ArtifactRefDto ToDto(tbl_ArtifactRef r, tbl_Artifact? artifact, AppUser? exposedBy) => new()
    {
        Id = r.Id,
        ArtifactId = r.ArtifactId,
        ProjectId = r.ProjectId,
        TargetType = r.TargetType,
        TargetId = r.TargetId,
        Channel = r.Channel,
        Caption = r.Caption,
        DisplayOrder = r.DisplayOrder,
        ExposedById = r.ExposedById,
        ExposedByName = exposedBy is not null
            ? $"{exposedBy.FirstName} {exposedBy.LastName}".Trim() : null,
        ExposedAt = r.ExposedAt,
        DateTimeCreated = r.DateTimeCreated,
        Artifact = artifact is null ? null : ToDto(artifact, 0)
    };

    private static DocumentDto ToDto(tbl_Document d, ICollection<tbl_ArtifactRevision> revisions) => new()
    {
        Id = d.Id,
        ProjectId = d.ProjectId,
        Kind = d.Kind,
        Title = d.Title,
        Channel = d.Channel,
        CurrentRevisionId = d.CurrentRevisionId,
        DateTimeCreated = d.DateTimeCreated,
        Revisions = revisions
            .OrderByDescending(r => r.RevisionNo)
            .Select(r => new ArtifactRevisionDto
            {
                Id = r.Id,
                DocumentId = r.DocumentId,
                ArtifactId = r.ArtifactId,
                RevisionNo = r.RevisionNo,
                IssuedById = r.IssuedById,
                IssuedByName = r.IssuedBy is not null
                    ? $"{r.IssuedBy.FirstName} {r.IssuedBy.LastName}".Trim() : null,
                IssuedAt = r.IssuedAt,
                Notes = r.Notes,
                SupersededByRevisionId = r.SupersededByRevisionId,
                IsCurrent = r.Id == d.CurrentRevisionId,
                DateTimeCreated = r.DateTimeCreated,
                Artifact = r.Artifact is null ? null : ToDto(r.Artifact, 0)
            })
            .ToList()
    };

    private static string NormalizeMime(string? contentType, string? fileName)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType != "application/octet-stream")
            return contentType.Split(';')[0].Trim().ToLowerInvariant();

        return Path.GetExtension(fileName)?.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private static string ExtensionFor(string mime, string? fileName)
    {
        var fromName = Path.GetExtension(fileName);
        if (!string.IsNullOrEmpty(fromName) && fromName.Length <= 8)
            return fromName.ToLowerInvariant();

        return mime switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/heic" => ".heic",
            "image/gif" => ".gif",
            "application/pdf" => ".pdf",
            _ => ".bin"
        };
    }

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}
