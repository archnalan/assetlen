using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="IIngestDAL"/>
public class IngestDAL : IIngestDAL
{
    private readonly AssetlenDbContext _context;
    private readonly ILogger<IngestDAL> _logger;
    private readonly IArtifactDAL _artifacts;
    private readonly IArtifactStorage _storage;
    private readonly IProjectAccessService _access;
    private readonly IConfiguration _config;

    /// <summary>
    /// A year of photos is a large file. Well above the 40 MB single-artifact
    /// ceiling because this is one archive standing in for hundreds of uploads.
    /// </summary>
    public const long MaxArchiveBytes = 512L * 1024 * 1024;

    /// <summary>Rows per SaveChanges during a bulk import. Keeps the change tracker from becoming the bottleneck.</summary>
    private const int SaveChunkSize = 500;

    /// <summary>
    /// Field separator inside a dedupe identity — ASCII 31, "unit separator".
    /// <para>
    /// Written as a code point rather than as a literal because the character is
    /// invisible in an editor: a copy-paste that drops it would leave the fields
    /// running together, and two different messages could then hash alike. It
    /// must be something no message body can contain, which rules out every
    /// printable choice.
    /// </para>
    /// </summary>
    private const char Sep = (char)0x1F;

    public IngestDAL(
        AssetlenDbContext context,
        ILogger<IngestDAL> logger,
        IArtifactDAL artifacts,
        IArtifactStorage storage,
        IProjectAccessService access,
        IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _artifacts = artifacts;
        _storage = storage;
        _access = access;
        _config = config;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Preview — read the export, write nothing
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<IngestPreviewDto>> PreviewArchiveAsync(
        Stream archive, string? fileName, string? contentType,
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(projectId))
                return Fail<IngestPreviewDto>(new BadRequestException("ProjectId is required."));

            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null)
                return Fail<IngestPreviewDto>(new NotFoundException("Project not found."));

            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanWrite)
                return Fail<IngestPreviewDto>(new ForbiddenException("Access denied."));

            await using var buffer = new MemoryStream();
            await archive.CopyToAsync(buffer, ct);
            if (buffer.Length == 0)
                return Fail<IngestPreviewDto>(new BadRequestException("The upload was empty."));
            if (buffer.Length > MaxArchiveBytes)
                return Fail<IngestPreviewDto>(new BadRequestException(
                    $"The archive exceeds the {MaxArchiveBytes / (1024 * 1024)} MB limit."));

            // Store the export itself as an artifact. Law 2 applied one level up:
            // the same export uploaded twice is one file, and the commit step
            // re-reads bytes from the store rather than holding them between calls.
            buffer.Position = 0;
            var stored = await _artifacts.IngestAsync(
                buffer, fileName, contentType, projectId, userId, DateTime.UtcNow, ct);
            if (!stored.IsSuccess)
                return ServiceResult<IngestPreviewDto>.Failure(stored.Error);

            buffer.Position = 0;
            using var export = IngestArchive.Open(buffer);

            if (!export.HasTranscript)
                return Fail<IngestPreviewDto>(new BadRequestException(
                    "No chat transcript found. Upload the .txt WhatsApp exports, or the .zip containing it."));

            var parsed = WhatsAppExportParser.Parse(export.ReadTranscript());
            if (parsed.Messages.Count == 0)
                return Fail<IngestPreviewDto>(new BadRequestException(
                    string.Join(" ", parsed.Warnings.DefaultIfEmpty("The transcript contained no messages."))));

            var keys = ComputeDedupeKeys(parsed.Messages);

            // How much of this export is already here. Asked before anything is
            // written, because "you already have 1,400 of these" is the answer
            // that makes a second import safe to attempt.
            var existingKeys = await ExistingKeysAsync(projectId, keys, ct);

            var available = parsed.Messages.Count(m => export.HasMedia(m.MediaFileName));
            var missing = parsed.Messages.Count(m => m.HasMediaMarker && !export.HasMedia(m.MediaFileName));

            var batch = new tbl_IngestBatch
            {
                ProjectId = projectId,
                TenantId = project.OwnerTenantId,
                SourceType = IngestSourceType.WhatsAppExport,
                Status = IngestBatchStatus.Previewed,
                ArchiveArtifactId = stored.Data!.Id,
                OriginalFileName = Truncate(fileName, 260),
                ImportedById = userId,
                ImportedSide = access.Side ?? ProjectSide.Client,
                StartedAt = DateTime.UtcNow,
                ParsedMessageCount = parsed.Messages.Count,
                DuplicateMessageCount = existingKeys.Count,
                MediaMessageCount = parsed.Messages.Count(m => m.HasMediaMarker),
                UnmatchedMediaCount = missing,
                ParticipantCount = parsed.Participants.Count,
                FirstMessageAt = parsed.Messages.Min(m => m.SentAt),
                LastMessageAt = parsed.Messages.Max(m => m.SentAt),
                DateOrder = parsed.DateOrder,
                Notes = JoinNotes(parsed.Warnings)
            };

            _context.tbl_IngestBatches.Add(batch);
            await _context.SaveChangesAsync(ct);

            var participants = await BuildParticipantsAsync(projectId, parsed, ct);

            return ServiceResult<IngestPreviewDto>.Success(new IngestPreviewDto
            {
                BatchId = batch.Id,
                ProjectId = projectId,
                OriginalFileName = batch.OriginalFileName,
                SourceType = IngestSourceType.WhatsAppExport,
                ArchiveArtifactId = batch.ArchiveArtifactId,
                ImportedSide = batch.ImportedSide,
                MessageCount = parsed.Messages.Count,
                AlreadyImportedCount = existingKeys.Count,
                NewMessageCount = parsed.Messages.Count - existingKeys.Count,
                MediaMessageCount = batch.MediaMessageCount,
                MediaFilesAvailable = available,
                MediaFilesMissing = missing,
                FirstMessageAt = batch.FirstMessageAt,
                LastMessageAt = batch.LastMessageAt,
                DateOrder = parsed.DateOrder,
                Participants = participants,
                Warnings = parsed.Warnings
            });
        }
        catch (InvalidDataException ex)
        {
            // A truncated or corrupt zip. Distinct from a server fault: the user
            // can act on it by re-exporting, so say so rather than returning 500.
            _logger.LogWarning(ex, "Corrupt ingest archive for project {ProjectId}", projectId);
            return Fail<IngestPreviewDto>(new BadRequestException(
                "That archive could not be opened. Re-export the chat and try again."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing ingest archive for project {ProjectId}", projectId);
            return Fail<IngestPreviewDto>(new ServerErrorException(ex.Message));
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // Commit — map the authors, store the media, write the messages
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<IngestBatchDto>> CommitImportAsync(
        IngestCommitDto dto, string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(dto.BatchId))
            return Fail<IngestBatchDto>(new BadRequestException("BatchId is required."));

        tbl_IngestBatch? batch = null;

        try
        {
            batch = await _context.tbl_IngestBatches
                .FirstOrDefaultAsync(b => b.Id == dto.BatchId, ct);
            if (batch is null)
                return Fail<IngestBatchDto>(new NotFoundException("Import not found."));

            if (batch.Status == IngestBatchStatus.Completed)
                return Fail<IngestBatchDto>(new ConflictException("This import has already been applied."));

            // Tracked, not AsNoTracking, and deliberately so: UpdateTimestamps
            // resolves each new row's owning tenant and falls back to a database
            // query per row when the project is not already in the change tracker.
            // Across 1,529 messages that is 1,529 extra round trips.
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == batch.ProjectId, ct);
            if (project is null)
                return Fail<IngestBatchDto>(new NotFoundException("Project not found."));

            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanWrite)
                return Fail<IngestBatchDto>(new ForbiddenException("Access denied."));

            var archiveStream = await OpenArchiveAsync(batch.ArchiveArtifactId, ct);
            if (archiveStream is null)
                return Fail<IngestBatchDto>(new NotFoundException(
                    "The uploaded archive is no longer available. Upload it again."));

            batch.Status = IngestBatchStatus.Importing;
            await _context.SaveChangesAsync(ct);

            await using (archiveStream)
            {
                using var export = IngestArchive.Open(archiveStream);
                var parsed = WhatsAppExportParser.Parse(export.ReadTranscript());

                var keys = ComputeDedupeKeys(parsed.Messages);
                var existing = await ExistingKeysAsync(batch.ProjectId!, keys, ct);

                var authorMap = await ResolveAuthorsAsync(batch.ProjectId!, dto.AuthorMappings, userId, ct);

                // Media first, in its own pass. ArtifactDAL saves internally, so
                // interleaving it with pending message rows would flush them
                // half-built; keeping the phases apart makes each save complete.
                var (artifactByIndex, newArtifacts, dupArtifacts) =
                    await StoreMediaAsync(parsed, export, batch, ct);

                var pending = new List<tbl_IngestedMessage>(SaveChunkSize);
                var imported = 0;

                for (var i = 0; i < parsed.Messages.Count; i++)
                {
                    if (existing.Contains(keys[i])) continue;

                    var m = parsed.Messages[i];
                    artifactByIndex.TryGetValue(i, out var artifactId);

                    pending.Add(new tbl_IngestedMessage
                    {
                        ProjectId = batch.ProjectId,
                        // Stamped explicitly for the same reason the project is
                        // tracked above — it short-circuits the per-row lookup.
                        TenantId = project.OwnerTenantId,
                        BatchId = batch.Id,
                        SourceType = IngestSourceType.WhatsAppExport,
                        ExternalAuthor = Truncate(m.Author, 200),
                        AuthorMemberId = m.Author.Length > 0
                            ? authorMap.GetValueOrDefault(m.Author)
                            : null,
                        SentAt = m.SentAt,
                        Body = m.Body,
                        ArtifactId = artifactId,
                        MediaFileName = Truncate(m.MediaFileName, 260),
                        IsSystemMessage = m.IsSystemMessage,
                        SequenceNo = m.SequenceNo,
                        DedupeKey = keys[i]
                    });

                    imported++;

                    if (pending.Count >= SaveChunkSize)
                    {
                        _context.tbl_IngestedMessages.AddRange(pending);
                        await _context.SaveChangesAsync(ct);
                        pending.Clear();
                    }
                }

                if (pending.Count > 0)
                {
                    _context.tbl_IngestedMessages.AddRange(pending);
                    await _context.SaveChangesAsync(ct);
                }

                batch.ParsedMessageCount = parsed.Messages.Count;
                batch.ImportedMessageCount = imported;
                batch.DuplicateMessageCount = existing.Count;
                batch.MediaMessageCount = parsed.Messages.Count(m => m.HasMediaMarker);
                batch.NewArtifactCount = newArtifacts;
                batch.DuplicateArtifactCount = dupArtifacts;
                batch.UnmatchedMediaCount = parsed.Messages.Count(
                    m => m.HasMediaMarker && !export.HasMedia(m.MediaFileName));
                batch.ParticipantCount = parsed.Participants.Count;
                batch.DateOrder = parsed.DateOrder;
                batch.Status = IngestBatchStatus.Completed;
                batch.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Ingest {BatchId}: {Imported} new of {Parsed} parsed, {NewArtifacts} new artifacts, " +
                    "{DupArtifacts} de-duplicated, {Missing} media markers with no file",
                    batch.Id, imported, parsed.Messages.Count, newArtifacts, dupArtifacts, batch.UnmatchedMediaCount);
            }

            return ServiceResult<IngestBatchDto>.Success(ToDto(batch, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing ingest batch {BatchId}", dto.BatchId);

            // A failed run is recorded, never rolled back into silence. A batch
            // that vanishes is indistinguishable from an export that held nothing.
            if (batch is not null)
            {
                try
                {
                    batch.Status = IngestBatchStatus.Failed;
                    batch.CompletedAt = DateTime.UtcNow;
                    batch.Notes = JoinNotes(new[] { batch.Notes, ex.Message });
                    await _context.SaveChangesAsync(ct);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Could not record the failure of batch {BatchId}", dto.BatchId);
                }
            }

            return Fail<IngestBatchDto>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Store every attachment the archive actually contains, and point a ref at
    /// each. Returns message index → artifact id, plus how many were new.
    /// </summary>
    private async Task<(Dictionary<int, string> ByIndex, int NewCount, int DuplicateCount)> StoreMediaAsync(
        WhatsAppParseResult parsed, IngestArchive export, tbl_IngestBatch batch, CancellationToken ct)
    {
        var byIndex = new Dictionary<int, string>();
        var newCount = 0;
        var duplicateCount = 0;

        // Ingest refs follow the side that imported them, which is a different
        // rule from the interactive one and deliberately so. AddRef lands
        // Crew-only because a delivery-side capture must wait for a mediator.
        // Nothing is crossing a boundary here: an uncurated view of Peter's own
        // forwarded material is exactly as private as his phone (assetlen.md §5).
        var channel = batch.ImportedSide == ProjectSide.Client ? Channel.Client : Channel.Crew;

        // The same bytes appear repeatedly in a real thread — a receipt sent five
        // times, a drawing re-shared each time it is asked for. Within one
        // archive we can skip the work entirely on the second sighting.
        var seenFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < parsed.Messages.Count; i++)
        {
            var m = parsed.Messages[i];
            if (string.IsNullOrEmpty(m.MediaFileName)) continue;

            if (seenFiles.TryGetValue(m.MediaFileName, out var already))
            {
                byIndex[i] = already;
                duplicateCount++;
                continue;
            }

            await using var media = export.OpenMedia(m.MediaFileName);
            if (media is null) continue;                       // export without media

            // Routed through ArtifactDAL rather than writing rows here: hash
            // matching, thumbnailing and the unique-index race are all solved
            // there, and a second implementation of Law 2 is a second place for
            // it to be wrong.
            var result = await _artifacts.IngestAsync(
                media, m.MediaFileName, null, batch.ProjectId!, batch.ImportedById!, m.SentAt, ct);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Could not store {File} from batch {BatchId}: {Error}",
                    m.MediaFileName, batch.Id, result.Error.Message);
                continue;
            }

            var artifact = result.Data!;
            byIndex[i] = artifact.Id!;
            seenFiles[m.MediaFileName] = artifact.Id!;

            if (artifact.WasDeduplicated) duplicateCount++; else newCount++;

            await EnsureRefAsync(artifact.Id!, batch, channel, m.MediaFileName, ct);
        }

        return (byIndex, newCount, duplicateCount);
    }

    /// <summary>
    /// Point the project at an ingested artifact, once. Idempotent against the
    /// unique index so a re-import adds no second pointer.
    /// </summary>
    private async Task EnsureRefAsync(
        string artifactId, tbl_IngestBatch batch, Channel channel, string caption, CancellationToken ct)
    {
        var exists = await _context.tbl_ArtifactRefs.AnyAsync(
            r => r.ArtifactId == artifactId
              && r.TargetType == ArtifactTargetType.IngestedMessage
              && r.TargetId == batch.ProjectId, ct);
        if (exists) return;

        _context.tbl_ArtifactRefs.Add(new tbl_ArtifactRef
        {
            ArtifactId = artifactId,
            ProjectId = batch.ProjectId,
            TenantId = batch.TenantId,
            TargetType = ArtifactTargetType.IngestedMessage,
            TargetId = batch.ProjectId,
            Channel = channel,
            Caption = Truncate(caption, 300),
            ExposedById = channel == Channel.Client ? batch.ImportedById : null,
            ExposedAt = channel == Channel.Client ? DateTime.UtcNow : null
        });

        await _context.SaveChangesAsync(ct);
    }

    // ═════════════════════════════════════════════════════════════════════
    // Author mapping
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Turn the caller's mapping choices into export-name → member-id, creating
    /// off-platform members where asked.
    /// <para>
    /// An unmapped participant is not an error. The messages land unattributed
    /// and can be claimed later; refusing the import would lose the record over
    /// a name, and the record is the thing worth having.
    /// </para>
    /// </summary>
    private async Task<Dictionary<string, string>> ResolveAuthorsAsync(
        string projectId, List<IngestAuthorMapDto> mappings, string actingUserId, CancellationToken ct)
    {
        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (mappings.Count == 0) return resolved;

        var members = await _context.tbl_ProjectMembers
            .Where(m => m.ProjectId == projectId)
            .ToListAsync(ct);

        foreach (var map in mappings)
        {
            if (string.IsNullOrWhiteSpace(map.ExternalAuthor)) continue;
            var author = map.ExternalAuthor.Trim();

            if (!string.IsNullOrEmpty(map.MemberId))
            {
                if (members.Any(m => m.Id == map.MemberId))
                    resolved[author] = map.MemberId;
                else
                    _logger.LogWarning("Ingest mapping named member {MemberId}, which is not on project {ProjectId}",
                        map.MemberId, projectId);
                continue;
            }

            if (string.IsNullOrWhiteSpace(map.CreateAsPartyName)) continue;

            var partyName = map.CreateAsPartyName.Trim();

            // Reuse an off-platform row with the same name rather than growing a
            // new one per import — otherwise re-importing a year makes a second
            // "Windows contractor" every time.
            var existing = members.FirstOrDefault(m =>
                m.UserId == null &&
                string.Equals(m.PartyName, partyName, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                resolved[author] = existing.Id!;
                continue;
            }

            var created = new tbl_ProjectMember
            {
                ProjectId = projectId,
                PartyName = Truncate(partyName, 200),
                Side = map.Side,
                Specialization = map.Specialization,
                IsMediator = false,      // never appointed by an import
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                AssignedById = actingUserId
            };

            _context.tbl_ProjectMembers.Add(created);
            await _context.SaveChangesAsync(ct);

            members.Add(created);
            resolved[author] = created.Id!;
        }

        return resolved;
    }

    /// <summary>
    /// Suggest a member for each export name. Suggestions are never applied on
    /// their own — attribution is the expensive thing to get wrong, so it stays
    /// a decision somebody makes.
    /// </summary>
    private async Task<List<IngestParticipantDto>> BuildParticipantsAsync(
        string projectId, WhatsAppParseResult parsed, CancellationToken ct)
    {
        var members = await _context.tbl_ProjectMembers
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId && m.IsActive)
            .AsNoTracking()
            .ToListAsync(ct);

        var alreadyMapped = await _context.tbl_IngestedMessages
            .Where(m => m.ProjectId == projectId && m.AuthorMemberId != null)
            .Select(m => m.ExternalAuthor!)
            .Distinct()
            .ToListAsync(ct);

        var mappedSet = new HashSet<string>(alreadyMapped, StringComparer.OrdinalIgnoreCase);

        var counts = parsed.Messages
            .Where(m => !m.IsSystemMessage && m.Author.Length > 0)
            .GroupBy(m => m.Author, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (Total: g.Count(), Media: g.Count(x => x.HasMediaMarker)),
                          StringComparer.OrdinalIgnoreCase);

        var result = new List<IngestParticipantDto>();

        foreach (var author in parsed.Participants)
        {
            var stats = counts.GetValueOrDefault(author);
            var match = MatchMember(members, author);

            result.Add(new IngestParticipantDto
            {
                ExternalAuthor = author,
                MessageCount = stats.Total,
                MediaCount = stats.Media,
                SuggestedMemberId = match?.Id,
                SuggestedMemberName = match is null ? null : DisplayName(match),
                AlreadyMapped = mappedSet.Contains(author)
            });
        }

        return result;
    }

    /// <summary>
    /// Match an export name to a member. Exact full name first, then a unique
    /// first-name hit — a thread names people as "Nalan", the roster as
    /// "Nalan Architect". Ambiguity yields no suggestion rather than a guess.
    /// </summary>
    private static tbl_ProjectMember? MatchMember(List<tbl_ProjectMember> members, string author)
    {
        var name = author.Trim();

        var exact = members.FirstOrDefault(m =>
            string.Equals(DisplayName(m), name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(m.PartyName, name, StringComparison.OrdinalIgnoreCase));
        if (exact is not null) return exact;

        var byFirstName = members.Where(m =>
            m.User is not null &&
            string.Equals(m.User.FirstName, name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (byFirstName.Count == 1) return byFirstName[0];

        var byPrefix = members.Where(m =>
            DisplayName(m).StartsWith(name + " ", StringComparison.OrdinalIgnoreCase)).ToList();
        return byPrefix.Count == 1 ? byPrefix[0] : null;
    }

    private static string DisplayName(tbl_ProjectMember member) =>
        member.User is not null
            ? $"{member.User.FirstName} {member.User.LastName}".Trim()
            : member.PartyName ?? string.Empty;

    // ═════════════════════════════════════════════════════════════════════
    // The ongoing trickle — share sheet and email-in
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<IngestedMessageDto>> CaptureShareAsync(
        Stream? file, string? fileName, string? contentType,
        ShareCaptureDto dto, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ProjectId))
                return Fail<IngestedMessageDto>(new BadRequestException("ProjectId is required."));

            if (file is null && string.IsNullOrWhiteSpace(dto.Text))
                return Fail<IngestedMessageDto>(new BadRequestException("Send a file, some text, or both."));

            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId, ct);
            if (project is null)
                return Fail<IngestedMessageDto>(new NotFoundException("Project not found."));

            var access = await _access.ResolveAsync(project, userId, ct);
            if (!access.CanWrite)
                return Fail<IngestedMessageDto>(new ForbiddenException("Access denied."));

            var side = access.Side ?? ProjectSide.Client;
            var sentAt = dto.SentAt ?? DateTime.UtcNow;

            var batch = await GetOrCreateTrickleBatchAsync(
                project, IngestSourceType.ShareSheet, userId, side, ct);

            string? artifactId = null;
            if (file is not null)
            {
                var stored = await _artifacts.IngestAsync(
                    file, fileName, contentType, project.Id!, userId, sentAt, ct);
                if (!stored.IsSuccess)
                    return ServiceResult<IngestedMessageDto>.Failure(stored.Error);

                artifactId = stored.Data!.Id;
                batch.MediaMessageCount++;
                if (stored.Data.WasDeduplicated) batch.DuplicateArtifactCount++;
                else batch.NewArtifactCount++;

                await EnsureRefAsync(artifactId!, batch,
                    side == ProjectSide.Client ? Channel.Client : Channel.Crew,
                    fileName ?? "Shared file", ct);
            }

            var author = await ResolveSelfAsync(project.Id!, userId, ct);

            var message = new tbl_IngestedMessage
            {
                ProjectId = project.Id,
                TenantId = project.OwnerTenantId,
                BatchId = batch.Id,
                SourceType = IngestSourceType.ShareSheet,
                ExternalAuthor = Truncate(author.Name, 200),
                AuthorMemberId = author.MemberId,
                SentAt = sentAt,
                Body = Truncate(dto.Text, WhatsAppExportParser.MaxBodyLength),
                ArtifactId = artifactId,
                MediaFileName = Truncate(fileName, 260),
                SequenceNo = batch.ImportedMessageCount
            };

            message.DedupeKey = DedupeKey(
                message.SentAt, message.ExternalAuthor, message.Body, message.MediaFileName, message.SequenceNo);

            _context.tbl_IngestedMessages.Add(message);

            batch.ImportedMessageCount++;
            batch.ParsedMessageCount++;
            batch.LastMessageAt = sentAt;
            batch.FirstMessageAt ??= sentAt;
            batch.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return ServiceResult<IngestedMessageDto>.Success(ToDto(message, null, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing a shared item into project {ProjectId}", dto.ProjectId);
            return Fail<IngestedMessageDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<IngestBatchDto>> ReceiveEmailAsync(
        InboundEmailDto dto, CancellationToken ct = default)
    {
        try
        {
            var key = ParseInboxKey(dto.To);
            if (string.IsNullOrEmpty(key))
                return Fail<IngestBatchDto>(new BadRequestException("The recipient address carries no project key."));

            // IgnoreQueryFilters: a mail relay has no tenant, so the ambient
            // filter would hide every project from this lookup.
            var project = await _context.tbl_Projects_RS
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.IngestEmailKey == key && p.IsDeleted != true, ct);
            if (project is null)
                return Fail<IngestBatchDto>(new NotFoundException("No project holds that address."));

            // Attribute to a member when the sender is one; otherwise the message
            // still lands, named by the address it came from.
            var sender = await _context.tbl_ProjectMembers
                .IgnoreQueryFilters()
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.ProjectId == project.Id
                                       && m.IsActive
                                       && m.User != null
                                       && m.User.Email == dto.From, ct);

            var side = sender?.Side ?? ProjectSide.Client;
            var sentAt = dto.SentAt ?? DateTime.UtcNow;

            var batch = await GetOrCreateTrickleBatchAsync(
                project, IngestSourceType.Email, sender?.UserId, side, ct);

            var author = sender is not null ? DisplayName(sender) : (dto.From ?? "Unknown sender");
            var body = string.IsNullOrWhiteSpace(dto.Subject)
                ? dto.TextBody
                : $"{dto.Subject}\n\n{dto.TextBody}".Trim();

            // One message for the mail, then one per attachment. Splitting them
            // means a forwarded receipt gets its own addressable row rather than
            // hiding inside a covering note.
            var written = 0;
            written += await AddTrickleMessageAsync(project, batch, author, body, null, null, sentAt, side, ct);

            foreach (var attachment in dto.Attachments)
            {
                if (string.IsNullOrEmpty(attachment.ContentBase64)) continue;

                byte[] bytes;
                try { bytes = Convert.FromBase64String(attachment.ContentBase64); }
                catch (FormatException)
                {
                    _logger.LogWarning("Inbound mail for project {ProjectId} carried an undecodable attachment {Name}",
                        project.Id, attachment.FileName);
                    continue;
                }

                await using var stream = new MemoryStream(bytes);
                var stored = await _artifacts.IngestAsync(
                    stream, attachment.FileName, attachment.ContentType,
                    project.Id!, sender?.UserId ?? project.InvestorId ?? string.Empty, sentAt, ct);

                if (!stored.IsSuccess)
                {
                    _logger.LogWarning("Could not store inbound attachment {Name}: {Error}",
                        attachment.FileName, stored.Error.Message);
                    continue;
                }

                if (stored.Data!.WasDeduplicated) batch.DuplicateArtifactCount++;
                else batch.NewArtifactCount++;

                batch.MediaMessageCount++;
                await EnsureRefAsync(stored.Data.Id!, batch,
                    side == ProjectSide.Client ? Channel.Client : Channel.Crew,
                    attachment.FileName ?? "Email attachment", ct);

                written += await AddTrickleMessageAsync(
                    project, batch, author, null, stored.Data.Id, attachment.FileName, sentAt, side, ct);
            }

            batch.ImportedMessageCount += written;
            batch.ParsedMessageCount += written;
            batch.LastMessageAt = sentAt;
            batch.FirstMessageAt ??= sentAt;
            batch.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return ServiceResult<IngestBatchDto>.Success(ToDto(batch, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving inbound email for {To}", dto.To);
            return Fail<IngestBatchDto>(new ServerErrorException(ex.Message));
        }
    }

    private async Task<int> AddTrickleMessageAsync(
        tbl_Project project, tbl_IngestBatch batch, string author, string? body,
        string? artifactId, string? fileName, DateTime sentAt, ProjectSide side, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body) && artifactId is null) return 0;

        var sequence = batch.ParsedMessageCount + batch.ImportedMessageCount;

        var message = new tbl_IngestedMessage
        {
            ProjectId = project.Id,
            TenantId = project.OwnerTenantId,
            BatchId = batch.Id,
            SourceType = batch.SourceType,
            ExternalAuthor = Truncate(author, 200),
            SentAt = sentAt,
            Body = Truncate(body, WhatsAppExportParser.MaxBodyLength),
            ArtifactId = artifactId,
            MediaFileName = Truncate(fileName, 260),
            SequenceNo = sequence
        };

        message.DedupeKey = DedupeKey(sentAt, author, body, fileName, sequence);

        _context.tbl_IngestedMessages.Add(message);
        await _context.SaveChangesAsync(ct);
        return 1;
    }

    /// <summary>
    /// One open batch per source per day, so the trickle does not create a
    /// thousand one-message runs while still keeping share-sheet and email
    /// arrivals separately accountable.
    /// </summary>
    private async Task<tbl_IngestBatch> GetOrCreateTrickleBatchAsync(
        tbl_Project project, IngestSourceType source, string? userId, ProjectSide side, CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date;

        var batch = await _context.tbl_IngestBatches
            .IgnoreQueryFilters()
            .Where(b => b.ProjectId == project.Id
                     && b.SourceType == source
                     && b.ImportedSide == side
                     && b.StartedAt >= since
                     && b.IsDeleted != true)
            .OrderByDescending(b => b.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (batch is not null) return batch;

        batch = new tbl_IngestBatch
        {
            ProjectId = project.Id,
            TenantId = project.OwnerTenantId,
            SourceType = source,
            Status = IngestBatchStatus.Completed,
            ImportedById = userId,
            ImportedSide = side,
            StartedAt = DateTime.UtcNow,
            OriginalFileName = source == IngestSourceType.Email ? "Inbound mail" : "Shared items"
        };

        _context.tbl_IngestBatches.Add(batch);
        await _context.SaveChangesAsync(ct);
        return batch;
    }

    private async Task<(string Name, string? MemberId)> ResolveSelfAsync(
        string projectId, string userId, CancellationToken ct)
    {
        var member = await _context.tbl_ProjectMembers
            .Include(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive, ct);

        if (member is not null) return (DisplayName(member), member.Id);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        return (user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim(), null);
    }

    // ═════════════════════════════════════════════════════════════════════
    // The inbound address
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<ProjectInboxDto>> GetInboxAsync(
        string projectId, string userId, CancellationToken ct = default) =>
        await InboxAsync(projectId, userId, reset: false, ct);

    public async Task<ServiceResult<ProjectInboxDto>> ResetInboxAsync(
        string projectId, string userId, CancellationToken ct = default) =>
        await InboxAsync(projectId, userId, reset: true, ct);

    private async Task<ServiceResult<ProjectInboxDto>> InboxAsync(
        string projectId, string userId, bool reset, CancellationToken ct)
    {
        try
        {
            var project = await _context.tbl_Projects_RS
                .Include(p => p.ParentProject)
                .FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null)
                return Fail<ProjectInboxDto>(new NotFoundException("Project not found."));

            var access = await _access.ResolveAsync(project, userId, ct);

            // Reading the address is enough to post into the project, so handing
            // it out is a write decision, and revoking it is an owner's.
            if (reset ? !access.CanManage : !access.CanWrite)
                return Fail<ProjectInboxDto>(new ForbiddenException("Access denied."));

            if (reset || string.IsNullOrEmpty(project.IngestEmailKey))
            {
                project.IngestEmailKey = Guid.NewGuid().ToString("N")[..16];
                await _context.SaveChangesAsync(ct);
            }

            var stats = await _context.tbl_IngestBatches
                .Where(b => b.ProjectId == projectId && b.SourceType == IngestSourceType.Email)
                .Select(b => new { b.ImportedMessageCount, b.CompletedAt })
                .ToListAsync(ct);

            return ServiceResult<ProjectInboxDto>.Success(new ProjectInboxDto
            {
                ProjectId = projectId,
                EmailAddress = $"in+{project.IngestEmailKey}@{InboundDomain}",
                ReceivedCount = stats.Sum(s => s.ImportedMessageCount),
                LastReceivedAt = stats.Count == 0 ? null : stats.Max(s => s.CompletedAt)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving the inbox for project {ProjectId}", projectId);
            return Fail<ProjectInboxDto>(new ServerErrorException(ex.Message));
        }
    }

    private string InboundDomain => _config["Ingest:InboundDomain"] ?? "in.assetlen.app";

    /// <summary>
    /// Pull the project key out of a recipient address. Handles the plus-addressed
    /// form and a bare local part, and tolerates a display name wrapper.
    /// </summary>
    internal static string? ParseInboxKey(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var value = address.Trim();

        var open = value.LastIndexOf('<');
        if (open >= 0)
        {
            var close = value.IndexOf('>', open);
            if (close > open) value = value[(open + 1)..close];
        }

        var at = value.IndexOf('@');
        if (at <= 0) return null;

        var local = value[..at];
        var plus = local.IndexOf('+');
        var key = plus >= 0 ? local[(plus + 1)..] : local;

        key = key.Trim();
        return key.Length is >= 8 and <= 40 ? key : null;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Reading the raw record
    // ═════════════════════════════════════════════════════════════════════

    public async Task<ServiceResult<List<IngestBatchDto>>> GetBatchesAsync(
        string projectId, string userId, CancellationToken ct = default)
    {
        try
        {
            var access = await _access.ResolveAsync(projectId, userId, ct);
            if (!access.CanRead)
                return Fail<List<IngestBatchDto>>(new ForbiddenException("Access denied."));

            var batches = await _context.tbl_IngestBatches
                .Include(b => b.ImportedBy)
                .Where(b => b.ProjectId == projectId)
                .AsNoTracking()
                .OrderByDescending(b => b.StartedAt)
                .ToListAsync(ct);

            return ServiceResult<List<IngestBatchDto>>.Success(
                batches.Where(b => CanReadBatch(b, access, userId))
                       .Select(b => ToDto(b, b.ImportedBy))
                       .ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing ingest batches for project {ProjectId}", projectId);
            return Fail<List<IngestBatchDto>>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<IngestBatchDto>> GetBatchAsync(
        string batchId, string userId, CancellationToken ct = default)
    {
        try
        {
            var batch = await _context.tbl_IngestBatches
                .Include(b => b.ImportedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == batchId, ct);
            if (batch is null)
                return Fail<IngestBatchDto>(new NotFoundException("Import not found."));

            var access = await _access.ResolveAsync(batch.ProjectId, userId, ct);

            // 404, not 403, for the same reason the Site Log answers 404: a
            // refusal would confirm that an import exists on a project the
            // caller is not entitled to know about.
            if (!access.CanRead || !CanReadBatch(batch, access, userId))
                return Fail<IngestBatchDto>(new NotFoundException("Import not found."));

            return ServiceResult<IngestBatchDto>.Success(ToDto(batch, batch.ImportedBy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading ingest batch {BatchId}", batchId);
            return Fail<IngestBatchDto>(new ServerErrorException(ex.Message));
        }
    }

    public async Task<ServiceResult<IngestedMessagePageDto>> GetMessagesAsync(
        IngestedMessageQueryDto query, string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(query.ProjectId))
                return Fail<IngestedMessagePageDto>(new BadRequestException("ProjectId is required."));

            var access = await _access.ResolveAsync(query.ProjectId, userId, ct);
            if (!access.CanRead)
                return Fail<IngestedMessagePageDto>(new ForbiddenException("Access denied."));

            // Which runs this caller may read at all. Resolved to a list of ids
            // first so the message query stays a single index seek.
            var batches = await _context.tbl_IngestBatches
                .Where(b => b.ProjectId == query.ProjectId)
                .Select(b => new { b.Id, b.ImportedSide, b.ImportedById })
                .AsNoTracking()
                .ToListAsync(ct);

            var readable = batches
                .Where(b => CanReadSide(b.ImportedSide, b.ImportedById, access, userId))
                .Select(b => b.Id!)
                .ToHashSet();

            if (readable.Count == 0)
                return ServiceResult<IngestedMessagePageDto>.Success(new IngestedMessagePageDto
                {
                    Skip = query.Skip,
                    Take = query.Take
                });

            var q = _context.tbl_IngestedMessages
                .Include(m => m.AuthorMember).ThenInclude(a => a!.User)
                .Include(m => m.Artifact)
                .Where(m => m.ProjectId == query.ProjectId
                         && m.BatchId != null
                         && readable.Contains(m.BatchId))
                .AsNoTracking();

            if (!string.IsNullOrEmpty(query.BatchId))
                q = q.Where(m => m.BatchId == query.BatchId);

            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(m => m.Body != null && m.Body.Contains(query.Search));

            if (query.From.HasValue) q = q.Where(m => m.SentAt >= query.From.Value);
            if (query.To.HasValue) q = q.Where(m => m.SentAt <= query.To.Value);
            if (query.MediaOnly) q = q.Where(m => m.ArtifactId != null || m.MediaFileName != null);

            var total = await q.CountAsync(ct);

            var take = Math.Clamp(query.Take <= 0 ? 100 : query.Take, 1, 500);

            var rows = await q
                .OrderBy(m => m.SentAt).ThenBy(m => m.SequenceNo)
                .Skip(Math.Max(0, query.Skip))
                .Take(take)
                .ToListAsync(ct);

            return ServiceResult<IngestedMessagePageDto>.Success(new IngestedMessagePageDto
            {
                Messages = rows.Select(m => ToDto(m, m.AuthorMember, m.Artifact)).ToList(),
                TotalCount = total,
                Skip = Math.Max(0, query.Skip),
                Take = take
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading ingested messages for project {ProjectId}", query.ProjectId);
            return Fail<IngestedMessagePageDto>(new ServerErrorException(ex.Message));
        }
    }

    /// <summary>
    /// Who may read a run's messages.
    /// <para>
    /// Everything ingested is Site Log material (assetlen.md §5), so the
    /// contractor side and mediators read it. Beyond that, <b>the importing side
    /// owns it</b>: Peter's own forwarded record is readable by his side, and a
    /// delivery-side import is not — Peter has no business reading the crew's
    /// operational chatter (D5), even as account owner. The importer can always
    /// read back what they put in.
    /// </para>
    /// <para>
    /// Note what is deliberately absent: <c>CanManage</c> alone grants nothing
    /// here. Ownership answers <em>who holds a key</em>, not <em>who did what</em>
    /// (§10.1), and merging those two is the mistake that section names.
    /// </para>
    /// </summary>
    private static bool CanReadBatch(tbl_IngestBatch batch, ProjectAccess access, string userId) =>
        CanReadSide(batch.ImportedSide, batch.ImportedById, access, userId);

    private static bool CanReadSide(ProjectSide importedSide, string? importedById, ProjectAccess access, string userId) =>
        access.CanSeeSiteLog
        || access.Side == importedSide
        || (!string.IsNullOrEmpty(importedById) && importedById == userId);

    // ═════════════════════════════════════════════════════════════════════
    // Dedupe
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A stable identity per message, aligned by index with the parse.
    /// <para>
    /// The occurrence ordinal is what makes this correct on real data. Android
    /// stamps to the minute and the corpus's normal pattern is thirteen to
    /// eighteen photos inside one — same author, same minute, all bodied
    /// <c>&lt;Media omitted&gt;</c>. Hashing without an ordinal collapses those
    /// eighteen frames to one; hashing with it keeps them distinct and still
    /// makes a re-import of an overlapping export add nothing, because the same
    /// transcript always produces the same ordinals.
    /// </para>
    /// </summary>
    internal static List<string> ComputeDedupeKeys(List<WhatsAppMessage> messages)
    {
        var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
        var keys = new List<string>(messages.Count);

        foreach (var m in messages)
        {
            var identity = string.Join(Sep,
                m.SentAt.ToString("O"), m.Author, m.Body, m.MediaFileName ?? string.Empty);

            var occurrence = occurrences.GetValueOrDefault(identity);
            occurrences[identity] = occurrence + 1;

            keys.Add(Hash(identity + Sep + occurrence.ToString()));
        }

        return keys;
    }

    private static string DedupeKey(DateTime sentAt, string? author, string? body, string? fileName, int sequence) =>
        Hash(string.Join(Sep,
            sentAt.ToString("O"), author ?? string.Empty, body ?? string.Empty,
            fileName ?? string.Empty, sequence.ToString()));

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    /// <summary>
    /// Which of these keys are already on the project. Chunked because a year's
    /// export is 1,529 keys and a single <c>IN</c> clause that size is rejected
    /// by SQL Server's parameter limit.
    /// </summary>
    private async Task<HashSet<string>> ExistingKeysAsync(
        string projectId, List<string> keys, CancellationToken ct)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);

        foreach (var chunk in keys.Distinct(StringComparer.Ordinal).Chunk(500))
        {
            var hits = await _context.tbl_IngestedMessages
                .Where(m => m.ProjectId == projectId && m.DedupeKey != null && chunk.Contains(m.DedupeKey))
                .Select(m => m.DedupeKey!)
                .ToListAsync(ct);

            foreach (var hit in hits) found.Add(hit);
        }

        return found;
    }

    private async Task<Stream?> OpenArchiveAsync(string? artifactId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(artifactId)) return null;

        var path = await _context.tbl_Artifacts
            .Where(a => a.Id == artifactId)
            .Select(a => a.StoragePath)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(path)) return null;

        var stream = await _storage.OpenAsync(path, ct);
        if (stream is null || stream.CanSeek) return stream;

        // ZipArchive needs to seek. Local storage hands back a FileStream, but a
        // remote IArtifactStorage may not, so buffer rather than assume.
        var buffer = new MemoryStream();
        await using (stream) await stream.CopyToAsync(buffer, ct);
        buffer.Position = 0;
        return buffer;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Mapping
    // ═════════════════════════════════════════════════════════════════════

    private static ServiceResult<T> Fail<T>(Exception ex) => ServiceResult<T>.Failure(ex);

    private static IngestBatchDto ToDto(tbl_IngestBatch b, AppUser? importedBy) => new()
    {
        Id = b.Id,
        ProjectId = b.ProjectId,
        SourceType = b.SourceType,
        Status = b.Status,
        OriginalFileName = b.OriginalFileName,
        ArchiveArtifactId = b.ArchiveArtifactId,
        ImportedById = b.ImportedById,
        ImportedByName = importedBy is not null
            ? $"{importedBy.FirstName} {importedBy.LastName}".Trim() : null,
        ImportedSide = b.ImportedSide,
        StartedAt = b.StartedAt,
        CompletedAt = b.CompletedAt,
        ParsedMessageCount = b.ParsedMessageCount,
        ImportedMessageCount = b.ImportedMessageCount,
        DuplicateMessageCount = b.DuplicateMessageCount,
        MediaMessageCount = b.MediaMessageCount,
        NewArtifactCount = b.NewArtifactCount,
        DuplicateArtifactCount = b.DuplicateArtifactCount,
        UnmatchedMediaCount = b.UnmatchedMediaCount,
        ParticipantCount = b.ParticipantCount,
        FirstMessageAt = b.FirstMessageAt,
        LastMessageAt = b.LastMessageAt,
        DateOrder = b.DateOrder,
        Notes = b.Notes,
        DateTimeCreated = b.DateTimeCreated
    };

    private static IngestedMessageDto ToDto(
        tbl_IngestedMessage m, tbl_ProjectMember? author, tbl_Artifact? artifact) => new()
    {
        Id = m.Id,
        ProjectId = m.ProjectId,
        BatchId = m.BatchId,
        SourceType = m.SourceType,
        ExternalAuthor = m.ExternalAuthor,
        AuthorMemberId = m.AuthorMemberId,
        AuthorMemberName = author is null ? null : DisplayName(author),
        AuthorSide = author?.Side,
        SentAt = m.SentAt,
        Body = m.Body,
        ArtifactId = m.ArtifactId,
        MediaFileName = m.MediaFileName,
        ThumbnailUrl = artifact?.ThumbnailPath is null ? null : $"/api/Artifacts/{m.ArtifactId}/thumbnail",
        IsSystemMessage = m.IsSystemMessage,
        SequenceNo = m.SequenceNo,
        DateTimeCreated = m.DateTimeCreated
    };

    private static string? JoinNotes(IEnumerable<string?> parts)
    {
        var joined = string.Join('\n', parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        return string.IsNullOrEmpty(joined) ? null : Truncate(joined, 4000);
    }

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? value : value.Length <= max ? value : value[..max];
}
