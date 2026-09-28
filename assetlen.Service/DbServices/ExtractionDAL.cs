using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices.Extraction;
using assetlen.Service.FileProcessingServices.Ocr;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IExtractionDAL"/>
public class ExtractionDAL : IExtractionDAL
{
    /// <summary>
    /// plan.md P5: under roughly two-thirds accepted, narrow the trigger rather
    /// than ship a confidently wrong register. Judged only once enough has been decided.
    /// </summary>
    public const double AcceptThreshold = 2.0 / 3.0;
    public const int MinimumSample = 10;

    private readonly AssetlenDbContext _context;
    private readonly ILogger<ExtractionDAL> _logger;
    private readonly IProjectAccessService _access;
    private readonly ICommitmentDAL _commitments;
    private readonly IFlagDAL _flags;
    private readonly IArtifactDAL _artifacts;
    private readonly IArtifactTextQueue _textQueue;
    private readonly IOcrService _ocr;
    private readonly IReadOnlyList<IMessageExtractor> _extractors;
    private readonly IConfiguration _config;

    public ExtractionDAL(
        AssetlenDbContext context,
        ILogger<ExtractionDAL> logger,
        IProjectAccessService access,
        ICommitmentDAL commitments,
        IFlagDAL flags,
        IArtifactDAL artifacts,
        IArtifactTextQueue textQueue,
        IOcrService ocr,
        IEnumerable<IMessageExtractor> extractors,
        IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _access = access;
        _commitments = commitments;
        _flags = flags;
        _artifacts = artifacts;
        _textQueue = textQueue;
        _ocr = ocr;
        _extractors = extractors.ToList();
        _config = config;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Run
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ExtractionRunDto>> RunAsync(ExtractionRunRequestDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await LoadProject(dto.ProjectId, ct);
            if (project is null) return Fail<ExtractionRunDto>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanSeeRegister) return Fail<ExtractionRunDto>(new NotFoundException("Project not found."));
            if (!access.CanWrite) return Fail<ExtractionRunDto>(new ForbiddenException("You can read the queue but not add to it."));

            var started = DateTime.UtcNow;
            var batches = await ReadableBatchesAsync(project.Id!, access, userId, ct);
            var batchIds = batches.Keys.ToList();

            var rows = await _context.tbl_IngestedMessages.AsNoTracking()
                .Include(m => m.AuthorMember)
                .Where(m => m.ProjectId == project.Id && m.BatchId != null && batchIds.Contains(m.BatchId))
                .OrderBy(m => m.SentAt).ThenBy(m => m.SequenceNo)
                .Select(m => new { m.Id, m.SentAt, m.ExternalAuthor, Side = (ProjectSide?)m.AuthorMember!.Side, m.Body, m.IsSystemMessage, m.BatchId })
                .ToListAsync(ct);

            if (dto.From is { } from) rows = rows.Where(r => r.SentAt >= from).ToList();
            if (dto.To is { } to) rows = rows.Where(r => r.SentAt < to.Date.AddDays(1)).ToList();

            var inputs = rows.Select(r => new ExtractionInput(r.Id, r.SentAt, r.ExternalAuthor ?? "", r.Side, r.Body, r.IsSystemMessage)).ToList();
            var batchOf = rows.ToDictionary(r => r.Id, r => r.BatchId!);
            var sentAt = rows.ToDictionary(r => r.Id, r => r.SentAt);

            var extractor = ChooseExtractor();
            ExtractionOutput output;
            try
            {
                output = await extractor.ExtractAsync(inputs, ct);
            }
            catch (Exception ex) when (extractor is not RuleMessageExtractor)
            {
                _logger.LogWarning(ex, "Extractor {Engine} failed; falling back to the rules", extractor.Engine);
                extractor = _extractors.OfType<RuleMessageExtractor>().First();
                output = await extractor.ExtractAsync(inputs, ct);
                output.Notes.Add("The model was unavailable; the rules read the record instead.");
            }

            var stages = await _context.tbl_Stages.AsNoTracking()
                .Where(s => s.ProjectId == project.Id)
                .Select(s => new StageRef(s.Id, s.StageName ?? "", s.CatalogueKey))
                .ToListAsync(ct);

            var existing = (await _context.tbl_ExtractionProposals.IgnoreQueryFilters()
                .Where(p => p.ProjectId == project.Id)
                .Select(p => p.Fingerprint)
                .ToListAsync(ct)).ToHashSet();

            var run = new tbl_ExtractionRun
            {
                ProjectId = project.Id,
                TenantId = project.OwnerTenantId,
                StartedById = userId,
                StartedAt = started,
                Engine = extractor.Engine,
                MessagesRead = inputs.Count
            };
            _context.tbl_ExtractionRuns.Add(run);
            await _context.SaveChangesAsync(ct);

            foreach (var c in output.Candidates)
            {
                var fingerprint = Fingerprint(c);
                if (!existing.Add(fingerprint)) continue;

                var batch = batches[batchOf[c.MessageId]];
                _context.tbl_ExtractionProposals.Add(new tbl_ExtractionProposal
                {
                    ProjectId = project.Id,
                    TenantId = project.OwnerTenantId,
                    RunId = run.Id,
                    IngestedMessageId = c.MessageId,
                    Fingerprint = fingerprint,
                    Kind = c.Kind,
                    Title = Cap(c.Title, 200),
                    Detail = Cap(c.Detail, 2000),
                    Amount = c.Amount,
                    Currency = c.Currency,
                    DueDate = c.DueDate,
                    DateText = Cap(c.DateText, 100),
                    Quantity = Cap(c.Quantity, 100),
                    Maturity = c.Contested ? CommitmentMaturity.InDiscussion : c.Maturity,
                    OwedBySide = c.OwedBySide,
                    PartyName = Cap(c.PartyName, 200),
                    StageId = StageMatcher.Match(c.Title, stages),
                    Rule = c.Rule,
                    Engine = extractor.Engine,
                    Confidence = c.Confidence,
                    Contested = c.Contested,
                    ContestNote = Cap(c.ContestNote, 500),
                    SourceSide = batch.Side,
                    SourceImportedById = batch.ImportedById,
                    SourceSentAt = sentAt[c.MessageId]
                });
                run.ProposalsCreated++;
            }

            var readingKeys = (await _context.tbl_ProgressReadings.IgnoreQueryFilters()
                .Where(r => r.ProjectId == project.Id && r.SourceKind == ProgressReadingSource.Ingested)
                .Select(r => new { r.SourceId, r.Subject })
                .ToListAsync(ct))
                .Select(r => $"{r.SourceId}|{r.Subject}")
                .ToHashSet();

            foreach (var r in output.Readings)
            {
                var subject = Cap(r.Subject, 200)!;
                if (!readingKeys.Add($"{r.MessageId}|{subject}")) continue;

                var batch = batches[batchOf[r.MessageId]];
                _context.tbl_ProgressReadings.Add(new tbl_ProgressReading
                {
                    ProjectId = project.Id,
                    TenantId = project.OwnerTenantId,
                    StageId = StageMatcher.Match(r.Subject, stages),
                    Subject = subject,
                    Percent = r.Percent,
                    ObservedAt = r.ObservedAt,
                    SourceKind = ProgressReadingSource.Ingested,
                    SourceId = r.MessageId,
                    SourceSide = batch.Side,
                    SourceImportedById = batch.ImportedById
                });
                run.ReadingsCreated++;
            }

            run.CompletedAt = DateTime.UtcNow;
            run.Notes = Cap(string.Join("\n", output.Notes), 2000);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Extraction {RunId} on {ProjectId} with {Engine}: {Messages} messages, {Proposals} new proposals, {Readings} new readings",
                run.Id, project.Id, run.Engine, run.MessagesRead, run.ProposalsCreated, run.ReadingsCreated);

            return ServiceResult<ExtractionRunDto>.Success(ToDto(run));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Extraction failed for {ProjectId}", dto.ProjectId);
            return Fail<ExtractionRunDto>(new ServerErrorException(ex.Message));
        }
    }

    private IMessageExtractor ChooseExtractor()
    {
        var wanted = _config["Extraction:Engine"]?.Trim().ToLowerInvariant() ?? "auto";
        var rules = _extractors.OfType<RuleMessageExtractor>().First();
        if (wanted == "rules") return rules;
        return _extractors.FirstOrDefault(e => e is not RuleMessageExtractor && e.IsAvailable) ?? rules;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Queue
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ExtractionQueueDto>> GetQueueAsync(string projectId, ProposalStatus? status, string userId, CancellationToken ct = default)
    {
        try
        {
            var project = await LoadProject(projectId, ct);
            if (project is null) return Fail<ExtractionQueueDto>(new NotFoundException("Project not found."));
            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanSeeRegister) return Fail<ExtractionQueueDto>(new NotFoundException("Project not found."));

            var all = await _context.tbl_ExtractionProposals.AsNoTracking()
                .Include(p => p.Stage)
                .Include(p => p.DecidedBy)
                .Include(p => p.IngestedMessage)
                .Where(p => p.ProjectId == projectId)
                .ToListAsync(ct);

            var visible = all.Where(p => CanReadSource(p.SourceSide, p.SourceImportedById, access, userId)).ToList();

            var accepted = visible.Count(p => p.Status == ProposalStatus.Accepted);
            var rejected = visible.Count(p => p.Status == ProposalStatus.Rejected);
            double? rate = accepted + rejected == 0 ? null : (double)accepted / (accepted + rejected);

            var lastRun = await _context.tbl_ExtractionRuns.AsNoTracking()
                .Where(r => r.ProjectId == projectId)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            var readingCount = await ReadableReadings(projectId, access, userId).CountAsync(ct);

            var shown = visible
                .Where(p => status is null || p.Status == status)
                .OrderBy(p => p.Status)
                .ThenBy(p => p.SourceSentAt)
                .ToList();

            return ServiceResult<ExtractionQueueDto>.Success(new ExtractionQueueDto
            {
                Proposals = shown.Select(p => ToDto(p, access)).ToList(),
                PendingCount = visible.Count(p => p.Status == ProposalStatus.Pending),
                AcceptedCount = accepted,
                RejectedCount = rejected,
                AcceptRate = rate,
                Threshold = AcceptThreshold,
                MinimumSample = MinimumSample,
                BelowThreshold = rate is not null && accepted + rejected >= MinimumSample && rate < AcceptThreshold,
                Rules = visible.GroupBy(p => p.Rule ?? "unknown")
                    .Select(g =>
                    {
                        var a = g.Count(p => p.Status == ProposalStatus.Accepted);
                        var r = g.Count(p => p.Status == ProposalStatus.Rejected);
                        return new ExtractionRuleStatDto
                        {
                            Rule = g.Key, Proposed = g.Count(), Accepted = a, Rejected = r,
                            AcceptRate = a + r == 0 ? null : (double)a / (a + r)
                        };
                    })
                    .OrderBy(s => s.AcceptRate ?? 2).ThenByDescending(s => s.Proposed)
                    .ToList(),
                LastRun = lastRun is null ? null : ToDto(lastRun),
                ReadingCount = readingCount,
                CanDecide = access.CanWrite
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading the extraction queue for {ProjectId}", projectId);
            return Fail<ExtractionQueueDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Decide
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ProposalDecisionResultDto>> DecideAsync(ProposalDecisionDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var ids = dto.ProposalIds.Where(i => !string.IsNullOrWhiteSpace(i)).Distinct().ToList();
            if (ids.Count == 0) return Fail<ProposalDecisionResultDto>(new BadRequestException("Choose at least one proposal."));
            if (ids.Count > 500) return Fail<ProposalDecisionResultDto>(new BadRequestException("At most 500 at a time."));

            var proposals = await _context.tbl_ExtractionProposals
                .Include(p => p.IngestedMessage)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(ct);

            var result = new ProposalDecisionResultDto();
            var accessByProject = new Dictionary<string, ProjectAccess>();

            foreach (var p in proposals)
            {
                if (!accessByProject.TryGetValue(p.ProjectId!, out var access))
                    accessByProject[p.ProjectId!] = access = await _access.ResolveAsync(p.ProjectId, userId, ct);

                if (!access.CanSeeRegister || !CanReadSource(p.SourceSide, p.SourceImportedById, access, userId))
                {
                    result.Skipped++;
                    continue;
                }
                if (!access.CanWrite)
                {
                    result.Errors.Add("You can read the queue but not decide it.");
                    result.Skipped++;
                    continue;
                }
                if (p.Status != ProposalStatus.Pending)
                {
                    result.Skipped++;
                    continue;
                }

                if (!dto.Accept)
                {
                    p.Status = ProposalStatus.Rejected;
                    p.DecidedById = userId;
                    p.DecidedAt = DateTime.UtcNow;
                    result.Rejected++;
                    continue;
                }

                var written = await AcceptAsync(p, userId);
                if (written.Error is not null)
                {
                    result.Errors.Add($"{p.Title}: {written.Error}");
                    result.Skipped++;
                    continue;
                }

                p.Status = ProposalStatus.Accepted;
                p.DecidedById = userId;
                p.DecidedAt = DateTime.UtcNow;
                p.CommitmentId = written.CommitmentId;
                p.FlagId = written.FlagId;
                if (written.CommitmentId is not null) result.CommitmentIds.Add(written.CommitmentId);
                if (written.FlagId is not null) result.FlagIds.Add(written.FlagId);
                result.Accepted++;
            }

            result.Skipped += ids.Count - proposals.Count;
            await _context.SaveChangesAsync(ct);
            return ServiceResult<ProposalDecisionResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deciding proposals");
            return Fail<ProposalDecisionResultDto>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Write what the proposal describes through the same services a person
    /// uses, so every rule of the register — side, seat, accountable face,
    /// "nothing floats" — applies to extracted items exactly as to typed ones.
    /// </summary>
    private async Task<(string? CommitmentId, string? FlagId, string? Error)> AcceptAsync(tbl_ExtractionProposal p, string userId)
    {
        var source = Provenance(p);

        if (p.Kind == ProposalKind.Blocker)
        {
            var flag = await _flags.AddFlag(new FlagCreateDto
            {
                ProjectId = p.ProjectId,
                StageId = p.StageId,
                Title = p.Title,
                Description = Cap(source, 2000),
                Severity = FlagSeverity.Medium,
                // A blocker read from the client's own record stays on the client's side of the line.
                Channel = p.SourceSide == ProjectSide.Client ? Channel.Client : Channel.Crew,
                OwnerPartyName = p.PartyName
            }, userId);

            return flag.IsSuccess ? (null, flag.Data!.Id, null) : (null, null, flag.Error.Message);
        }

        var body = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(p.Quantity)) body.Append("Quantity: ").Append(p.Quantity).Append(". ");
        if (p.Contested && !string.IsNullOrWhiteSpace(p.ContestNote)) body.Append("Contested in the thread — ").Append(p.ContestNote).Append(". ");
        body.Append(source);

        var created = await _commitments.AddCommitment(new CommitmentCreateDto
        {
            ProjectId = p.ProjectId,
            StageId = p.StageId,
            Kind = (CommitmentKind)(int)p.Kind,
            Title = p.Title,
            Body = Cap(body.ToString(), 2000),
            Maturity = p.Maturity,
            SourceChannel = CommitmentSource.Ingested,
            AgreedAt = p.SourceSentAt,
            AgreedWithPartyName = p.PartyName,
            Amount = p.Amount,
            Currency = p.Currency,
            DueDate = p.DueDate,
            OwedBySide = p.OwedBySide,
            IngestedMessageId = p.IngestedMessageId
        }, userId);

        return created.IsSuccess ? (created.Data!.Id, null, null) : (null, null, created.Error.Message);
    }

    public async Task<ServiceResult<ExtractionProposalDto>> EditAsync(ProposalEditDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            var p = await _context.tbl_ExtractionProposals
                .Include(x => x.Stage).Include(x => x.IngestedMessage)
                .FirstOrDefaultAsync(x => x.Id == dto.ProposalId, ct);
            if (p is null) return Fail<ExtractionProposalDto>(new NotFoundException("Proposal not found."));

            var access = await _access.ResolveAsync(p.ProjectId, userId, ct);
            if (!access.CanSeeRegister || !CanReadSource(p.SourceSide, p.SourceImportedById, access, userId))
                return Fail<ExtractionProposalDto>(new NotFoundException("Proposal not found."));
            if (!access.CanWrite) return Fail<ExtractionProposalDto>(new ForbiddenException("You can read the queue but not change it."));
            if (p.Status != ProposalStatus.Pending) return Fail<ExtractionProposalDto>(new ConflictException("Already decided."));

            if (!string.IsNullOrWhiteSpace(dto.Title)) p.Title = Cap(dto.Title.Trim(), 200);
            if (dto.Kind is { } kind && Enum.IsDefined(kind)) p.Kind = kind;
            if (dto.Amount is not null && access.CanSeeMoney) p.Amount = dto.Amount;
            if (dto.DueDate is not null) p.DueDate = dto.DueDate;
            if (dto.StageId is not null)
            {
                var ok = await _context.tbl_Stages.AnyAsync(s => s.Id == dto.StageId && s.ProjectId == p.ProjectId, ct);
                if (!ok) return Fail<ExtractionProposalDto>(new BadRequestException("That stage is not on this project."));
                p.StageId = dto.StageId;
            }

            await _context.SaveChangesAsync(ct);
            await _context.Entry(p).Reference(x => x.Stage).LoadAsync(ct);
            return ServiceResult<ExtractionProposalDto>.Success(ToDto(p, access));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing proposal {Id}", dto.ProposalId);
            return Fail<ExtractionProposalDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Readings and OCR
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<ProgressReadingDto>>> GetReadingsAsync(string projectId, string? stageId, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanRead) return Fail<List<ProgressReadingDto>>(new NotFoundException("Project not found."));

            var q = ReadableReadings(projectId, access, userId);
            if (!string.IsNullOrEmpty(stageId)) q = q.Where(r => r.StageId == stageId);

            var rows = await q.Include(r => r.Stage).OrderBy(r => r.ObservedAt).ToListAsync(ct);
            return ServiceResult<List<ProgressReadingDto>>.Success(rows.Select(r => new ProgressReadingDto
            {
                Id = r.Id,
                ProjectId = r.ProjectId,
                StageId = r.StageId,
                StageName = r.Stage?.StageName,
                Subject = r.Subject,
                Percent = r.Percent,
                ObservedAt = r.ObservedAt,
                SourceKind = r.SourceKind,
                SourceId = r.SourceId,
                DateTimeCreated = r.DateTimeCreated
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading progress readings for {ProjectId}", projectId);
            return Fail<List<ProgressReadingDto>>(new ServerErrorException(ex.Message));
        }
    }

    private IQueryable<tbl_ProgressReading> ReadableReadings(string projectId, ProjectAccess access, string userId)
    {
        var q = _context.tbl_ProgressReadings.AsNoTracking().Where(r => r.ProjectId == projectId);
        if (access.CanSeeSiteLog) return q;

        // Everyone on the project reads stage and manual readings; an ingested
        // one is readable by the side that imported it, as its message is.
        var side = access.Side;
        return q.Where(r => r.SourceSide == null || r.SourceSide == side || r.SourceImportedById == userId);
    }

    public async Task<ServiceResult<ArtifactTextDto>> GetArtifactTextAsync(string artifactId, string userId, CancellationToken ct = default)
    {
        try
        {
            // Visibility is the artifact's own: if the reader cannot see the file,
            // they cannot read what is written on it either.
            var artifact = await _artifacts.GetAsync(artifactId, userId, ct);
            if (!artifact.IsSuccess) return ServiceResult<ArtifactTextDto>.Failure(artifact.Error);

            var row = await _context.tbl_ArtifactTexts.AsNoTracking().FirstOrDefaultAsync(t => t.ArtifactId == artifactId, ct);
            return ServiceResult<ArtifactTextDto>.Success(row is null
                ? new ArtifactTextDto { ArtifactId = artifactId, Status = ArtifactTextStatus.Pending }
                : new ArtifactTextDto
                {
                    ArtifactId = artifactId,
                    Status = row.Status,
                    Engine = row.Engine,
                    Text = row.Text,
                    CharCount = row.CharCount,
                    ExtractedAt = row.ExtractedAt,
                    Error = row.Error
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading text for artifact {ArtifactId}", artifactId);
            return Fail<ArtifactTextDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<OcrQueueResultDto>> QueueProjectOcrAsync(string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanRead) return Fail<OcrQueueResultDto>(new NotFoundException("Project not found."));
            if (!access.CanWrite) return Fail<OcrQueueResultDto>(new ForbiddenException("Access denied."));

            var artifactIds = await _context.tbl_Artifacts.AsNoTracking()
                .Where(a => a.ProjectId == projectId)
                .Select(a => a.Id)
                .ToListAsync(ct);

            var settled = (await _context.tbl_ArtifactTexts.AsNoTracking()
                .Where(t => t.ProjectId == projectId
                            && (t.Status == ArtifactTextStatus.Done || t.Status == ArtifactTextStatus.NoText || t.Status == ArtifactTextStatus.Unsupported))
                .Select(t => t.ArtifactId!)
                .ToListAsync(ct)).ToHashSet();

            // Rows the job gave up on are reset so the next pass may try again.
            var retry = await _context.tbl_ArtifactTexts
                .Where(t => t.ProjectId == projectId && (t.Status == ArtifactTextStatus.EngineUnavailable || t.Status == ArtifactTextStatus.Failed))
                .ToListAsync(ct);
            foreach (var r in retry) r.Status = ArtifactTextStatus.Pending;
            await _context.SaveChangesAsync(ct);

            var queued = 0;
            foreach (var id in artifactIds.Where(id => !settled.Contains(id)))
            {
                _textQueue.Enqueue(id);
                queued++;
            }

            return ServiceResult<OcrQueueResultDto>.Success(new OcrQueueResultDto
            {
                Queued = queued,
                AlreadyRead = settled.Count,
                Engine = _ocr.ImageEngine,
                EngineAvailable = _ocr.ImageEngine is not null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing OCR for {ProjectId}", projectId);
            return Fail<OcrQueueResultDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════

    private sealed record BatchInfo(ProjectSide Side, string? ImportedById);

    private async Task<Dictionary<string, BatchInfo>> ReadableBatchesAsync(string projectId, ProjectAccess access, string userId, CancellationToken ct)
    {
        var batches = await _context.tbl_IngestBatches.AsNoTracking()
            .Where(b => b.ProjectId == projectId)
            .Select(b => new { b.Id, b.ImportedSide, b.ImportedById })
            .ToListAsync(ct);

        return batches
            .Where(b => CanReadSource(b.ImportedSide, b.ImportedById, access, userId))
            .ToDictionary(b => b.Id!, b => new BatchInfo(b.ImportedSide, b.ImportedById));
    }

    /// <summary>The read gate of the ingested record (IngestDAL), applied to what was read out of it.</summary>
    private static bool CanReadSource(ProjectSide side, string? importedById, ProjectAccess access, string userId) =>
        access.CanSeeSiteLog
        || access.Side == side
        || (!string.IsNullOrEmpty(importedById) && importedById == userId);

    private Task<tbl_Project?> LoadProject(string? projectId, CancellationToken ct) =>
        _context.tbl_Projects_RS.Include(p => p.ParentProject).FirstOrDefaultAsync(p => p.Id == projectId, ct);

    private static string Fingerprint(ExtractionCandidate c) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{c.MessageId}\u001f{c.Kind}\u001f{RuleMessageExtractor.Normalise(c.Title)}"))).ToLowerInvariant();

    private static string Provenance(tbl_ExtractionProposal p)
    {
        var m = p.IngestedMessage;
        if (m is null) return "Read from the imported thread.";
        return $"From the thread, {m.ExternalAuthor}, {m.SentAt:d MMM yyyy HH:mm}: “{Cap(m.Body?.Trim(), 600)}”";
    }

    private static ExtractionProposalDto ToDto(tbl_ExtractionProposal p, ProjectAccess access)
    {
        var hide = p.Amount is not null && !access.CanSeeMoney;
        return new ExtractionProposalDto
        {
            Id = p.Id,
            ProjectId = p.ProjectId,
            IngestedMessageId = p.IngestedMessageId,
            Kind = p.Kind,
            Title = p.Title,
            Detail = p.Detail,
            Amount = hide ? null : p.Amount,
            Currency = hide ? null : p.Currency,
            AmountHidden = hide,
            DueDate = p.DueDate,
            DateText = p.DateText,
            Quantity = p.Quantity,
            Maturity = p.Maturity,
            OwedBySide = p.OwedBySide,
            PartyName = p.PartyName,
            StageId = p.StageId,
            StageName = p.Stage?.StageName,
            StagePhase = (int)(p.Stage?.Phase ?? StageGroup.Custom),
            Rule = p.Rule,
            Engine = p.Engine,
            Confidence = p.Confidence,
            Contested = p.Contested,
            ContestNote = p.ContestNote,
            Status = p.Status,
            DecidedAt = p.DecidedAt,
            DecidedByName = p.DecidedBy is null ? null : $"{p.DecidedBy.FirstName} {p.DecidedBy.LastName}".Trim(),
            CommitmentId = p.CommitmentId,
            FlagId = p.FlagId,
            SourceAuthor = p.IngestedMessage?.ExternalAuthor,
            SourceSide = p.SourceSide,
            SourceSentAt = p.SourceSentAt,
            SourceExcerpt = Excerpt(p.IngestedMessage?.Body, p.Title),
            DateTimeCreated = p.DateTimeCreated
        };
    }

    private static ExtractionRunDto ToDto(tbl_ExtractionRun r) => new()
    {
        Id = r.Id,
        ProjectId = r.ProjectId,
        Engine = r.Engine,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt,
        MessagesRead = r.MessagesRead,
        ProposalsCreated = r.ProposalsCreated,
        ReadingsCreated = r.ReadingsCreated,
        Notes = r.Notes,
        DateTimeCreated = r.DateTimeCreated
    };

    /// <summary>
    /// The line of the source the proposal was read from. A five-item material
    /// schedule is one message; quoting its first line under item four would
    /// make the evidence look wrong when it is not.
    /// </summary>
    private static string? Excerpt(string? body, string? title)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        var lines = body.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        if (lines.Count <= 2 || string.IsNullOrWhiteSpace(title)) return Cap(body.Trim(), 400);

        var wanted = title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2).ToHashSet();
        var best = lines
            .Select((l, i) => (i, score: l.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(wanted.Contains)))
            .OrderByDescending(x => x.score).First();

        var line = lines[best.i];
        return Cap(best.i > 0 ? "… " + line : line, 400);
    }

    private static string? Cap(string? s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..(max - 1)] + "…";

    private static ServiceResult<T> Fail<T>(Exception e) => ServiceResult<T>.Failure(e);
}
