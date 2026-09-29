using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using assetlen.Service.DataAccess;
using assetlen.Service.DbServices.ServiceInterfaces;
using assetlen.Service.FileProcessingServices.Ocr;
using assetlen.Service.FileProcessingServices.Search;
using assetlen.ServiceHandler;
using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.RemoteSite;
using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Service.DbServices;

/// <inheritdoc cref="ISearchDAL"/>
/// <remarks>
/// <b>Matching.</b> Case-insensitive substring per term (the database collation
/// is CI), scored in memory. SQL Server Full-Text Search is not installed on the
/// dev instance, and the corpus per project is thousands of rows, not millions;
/// <see cref="CandidatesAsync{T}"/> is the one place a <c>CONTAINS</c> query
/// would replace the <c>LIKE</c> when it is. The server reports which one answered.
/// </remarks>
public class SearchDAL : ISearchDAL
{
    private const int CandidateCap = 400;

    private readonly AssetlenDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly IOcrService _ocr;
    private readonly ILogger<SearchDAL> _logger;

    private static bool? _fullTextInstalled;

    public SearchDAL(AssetlenDbContext context, IProjectAccessService access, IOcrService ocr, ILogger<SearchDAL> logger)
    {
        _context = context;
        _access = access;
        _ocr = ocr;
        _logger = logger;
    }

    // The per-call picture of what this reader stands on, built once and read by every source.
    private sealed class Scope
    {
        public required string UserId { get; init; }
        public required SearchTerms Terms { get; init; }
        public Dictionary<string, (tbl_Project Project, ProjectAccess Access)> Projects { get; } = new();
        public Dictionary<string, BatchRow> Batches { get; } = new();
        public HashSet<string> ReadableBatches { get; } = new();
        public HashSet<string> ArchiveArtifacts { get; } = new();
        public Dictionary<string, string> MediatorName { get; } = new();

        public List<string> ProjectIds => Projects.Keys.ToList();
        public ProjectAccess AccessOn(string? pid) => pid is not null && Projects.TryGetValue(pid, out var p) ? p.Access : ProjectAccess.None;
        public string? NameOf(string? pid) => pid is not null && Projects.TryGetValue(pid, out var p) ? p.Project.ProjectName : null;
    }

    private sealed record BatchRow(string Id, string ProjectId, ProjectSide Side, string? ImportedById, IngestSourceType Source);

    public async Task<ServiceResult<SearchResultDto>> SearchAsync(
        string? query, string? projectId, int take, string userId, CancellationToken ct = default)
    {
        try
        {
            var terms = SearchTerms.Parse(query);
            if (terms.Terms.Count == 0)
                return Fail(new BadRequestException("Type a word to look for."));

            take = Math.Clamp(take <= 0 ? 20 : take, 1, 100);

            var scope = new Scope { UserId = userId, Terms = terms };
            await LoadProjectsAsync(scope, projectId, ct);

            if (!string.IsNullOrEmpty(projectId) && scope.Projects.Count == 0)
                return Fail(new NotFoundException("Project not found."));

            await LoadBatchesAsync(scope, ct);
            await LoadMediatorsAsync(scope, ct);

            var commitments = await SearchCommitmentsAsync(scope, ct);
            var messages = await SearchMessagesAsync(scope, ct);
            var files = await SearchFilesAsync(scope, ct);
            var diary = await SearchDiaryAsync(scope, ct);
            var places = await SearchStagesAndProjectsAsync(scope, ct);

            // Grouped by object: a message that is where a commitment was said is
            // that commitment's evidence, not a second result about the same thing.
            await FoldMessagesIntoCommitmentsAsync(scope, messages, commitments, ct);

            await ProvenanceForCommitmentsAsync(scope, commitments, ct);

            var result = new SearchResultDto
            {
                Query = query?.Trim() ?? "",
                Terms = terms.Terms.ToList(),
                SetAside = terms.SetAside.ToList(),
                FullTextInstalled = await FullTextInstalledAsync(ct),
                Backend = "substring",
                ProjectsSearched = scope.Projects.Count,
                OcrEngine = _ocr.ImageEngine,
                FilesAwaitingText = await FilesAwaitingTextAsync(scope, ct),
                SearchedSiteDiary = scope.Projects.Values.Any(p => p.Access.CanSeeSiteLog)
            };

            AddGroup(result, SearchHitKind.Commitment, "Commitments", commitments.Values, take);
            AddGroup(result, SearchHitKind.File, "Photos and files", files, take);
            AddGroup(result, SearchHitKind.Message, "From the thread", messages, take);
            AddGroup(result, SearchHitKind.DiaryEntry, "Site Diary", diary, take);
            AddGroup(result, SearchHitKind.Stage, "Projects and stages", places, take);

            return ServiceResult<SearchResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for {Query}", query);
            return Fail(new ServerErrorException(ex.Message));
        }
    }

    private static void AddGroup(SearchResultDto result, SearchHitKind kind, string label, IEnumerable<SearchHitDto> hits, int take)
    {
        var all = hits.ToList();
        if (all.Count == 0) return;
        result.Groups.Add(new SearchGroupDto
        {
            Kind = kind,
            Label = label,
            Total = all.Count,
            Hits = all.OrderByDescending(h => h.Score).ThenByDescending(h => h.At ?? DateTime.MinValue).Take(take).ToList()
        });
    }

    // ═════════════════════════════════════════════════════════════════════
    // Standing
    // ═════════════════════════════════════════════════════════════════════

    private async Task LoadProjectsAsync(Scope scope, string? projectId, CancellationToken ct)
    {
        var userId = scope.UserId;

        // The dashboard's candidate rule; IProjectAccessService then decides.
        var q = _context.tbl_Projects_RS.AsNoTracking()
            .Include(p => p.ParentProject)
            .Where(p => p.ArchivedAt == null && (p.ParentProject == null || p.ParentProject.ArchivedAt == null))
            .Where(p => p.InvestorId == userId
                        || p.ProjectManagerId == userId
                        || (p.ParentProject != null && (p.ParentProject.InvestorId == userId || p.ParentProject.ProjectManagerId == userId))
                        || _context.tbl_ProjectMembers.Any(m => m.ProjectId == p.Id && m.UserId == userId && m.IsActive)
                        || (p.ParentProjectId != null && _context.tbl_ProjectMembers.Any(m => m.ProjectId == p.ParentProjectId && m.UserId == userId && m.IsActive)));

        if (!string.IsNullOrEmpty(projectId))
            q = q.Where(p => p.Id == projectId || p.ParentProjectId == projectId);

        var projects = await q.ToListAsync(ct);
        var standings = await _access.ResolveManyAsync(projects, userId, ct);

        foreach (var p in projects)
            if (standings.TryGetValue(p.Id, out var a) && a.CanRead)
                scope.Projects[p.Id] = (p, a);
    }

    private async Task LoadBatchesAsync(Scope scope, CancellationToken ct)
    {
        var pids = scope.ProjectIds;
        var rows = await _context.tbl_IngestBatches.AsNoTracking()
            .Where(b => b.ProjectId != null && pids.Contains(b.ProjectId))
            .Select(b => new { b.Id, b.ProjectId, b.ImportedSide, b.ImportedById, b.SourceType, b.ArchiveArtifactId })
            .ToListAsync(ct);

        foreach (var b in rows)
        {
            var row = new BatchRow(b.Id, b.ProjectId!, b.ImportedSide, b.ImportedById, b.SourceType);
            scope.Batches[b.Id] = row;
            if (CanReadThread(row, scope.AccessOn(row.ProjectId), scope.UserId)) scope.ReadableBatches.Add(b.Id);

            // The uploaded transcript is how the thread arrived, not something said
            // in it; its text is already searchable message by message.
            if (!string.IsNullOrEmpty(b.ArchiveArtifactId)) scope.ArchiveArtifacts.Add(b.ArchiveArtifactId);
        }
    }

    /// <summary>
    /// The history tab's rule (<c>ProjectAccess.CanSeeHistory</c>) with the
    /// importing side's own claim on what it forwarded (IngestDAL): the principals
    /// read the thread; the bench, which was brought on for one job, does not.
    /// </summary>
    private static bool CanReadThread(BatchRow b, ProjectAccess access, string userId) =>
        (!string.IsNullOrEmpty(b.ImportedById) && b.ImportedById == userId)
        || (access.CanRead && access.Seat == ProjectSeat.Principal && (access.CanSeeSiteLog || access.Side == b.Side));

    private async Task LoadMediatorsAsync(Scope scope, CancellationToken ct)
    {
        var pids = scope.ProjectIds;
        var mediators = await _context.tbl_ProjectMembers.AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.ProjectId != null && pids.Contains(m.ProjectId) && m.IsActive && m.IsMediator)
            .ToListAsync(ct);

        foreach (var m in mediators.GroupBy(m => m.ProjectId!))
            scope.MediatorName[m.Key] = MemberName(m.First());
    }

    // ═════════════════════════════════════════════════════════════════════
    // Sources
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ids matching at least one term, one query per term, unioned. The single
    /// seam where full-text <c>CONTAINS</c> would replace substring matching.
    /// </summary>
    private async Task<HashSet<string>> CandidatesAsync<T>(
        IQueryable<T> source,
        Func<string, System.Linq.Expressions.Expression<Func<T, bool>>> matches,
        System.Linq.Expressions.Expression<Func<T, string>> id,
        SearchTerms terms,
        CancellationToken ct)
    {
        var ids = new HashSet<string>();
        foreach (var t in terms.Terms)
        {
            var found = await source.Where(matches(t)).Select(id).Take(CandidateCap).ToListAsync(ct);
            ids.UnionWith(found);
        }
        return ids;
    }

    private async Task<Dictionary<string, SearchHitDto>> SearchCommitmentsAsync(Scope scope, CancellationToken ct)
    {
        // A commitment is addressed to a decision-maker; the bench has no register (CanSeeRegister).
        var pids = scope.Projects.Where(p => p.Value.Access.CanSeeRegister).Select(p => p.Key).ToList();
        var hits = new Dictionary<string, SearchHitDto>();
        if (pids.Count == 0) return hits;

        var ids = await CandidatesAsync(
            _context.tbl_Commitments.AsNoTracking().Where(c => c.ProjectId != null && pids.Contains(c.ProjectId) && c.SupersededAt == null),
            t => c => (c.Title != null && c.Title.Contains(t)) || (c.Body != null && c.Body.Contains(t))
                      || (c.AgreedWithPartyName != null && c.AgreedWithPartyName.Contains(t)),
            c => c.Id, scope.Terms, ct);

        foreach (var c in await LoadCommitmentsAsync(ids, ct))
        {
            var n = scope.Terms.CountIn(c.Title, c.Body, c.AgreedWithPartyName);
            if (n < scope.Terms.Required) continue;

            var hit = CommitmentHit(scope, c);
            var inTitle = scope.Terms.CountIn(c.Title) > 0;
            hit.MatchedIn = inTitle ? "its title" : scope.Terms.CountIn(c.Body) > 0 ? "what was agreed" : "who it was agreed with";
            hit.Snippet = inTitle ? scope.Terms.Snippet(c.Body) : scope.Terms.Snippet(c.Body ?? c.AgreedWithPartyName);
            hit.Score = Score(scope.Terms, n, inTitle, hit.At) + (scope.Terms.AsksForAgreement ? 3 : 1);
            hits[c.Id] = hit;
        }
        return hits;
    }

    private Task<List<tbl_Commitment>> LoadCommitmentsAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var list = ids.ToList();
        return _context.tbl_Commitments.AsNoTracking()
            .Include(c => c.Stage)
            .Include(c => c.AccountableMember).ThenInclude(m => m!.User)
            .Include(c => c.RecordedBy)
            .Where(c => list.Contains(c.Id))
            .ToListAsync(ct);
    }

    private SearchHitDto CommitmentHit(Scope scope, tbl_Commitment c)
    {
        var access = scope.AccessOn(c.ProjectId);
        var money = access.CanSeeMoney;

        // One accountable face (§10.1): the client side reads the mediator's
        // name on anything the bench wrote, never the bench's.
        var accountable = c.AccountableMember is null ? null : MemberName(c.AccountableMember);
        var hideAuthor = !access.CanSeeSiteLog && c.RecordedBySide == ProjectSide.Contractor;

        return new SearchHitDto
        {
            Id = c.Id,
            Kind = SearchHitKind.Commitment,
            ProjectId = c.ProjectId,
            ProjectName = scope.NameOf(c.ProjectId),
            StageId = c.StageId,
            StageName = c.Stage?.StageName,
            StagePhase = c.Stage?.Phase,
            Title = c.Title ?? "Commitment",
            At = c.AgreedAt ?? c.DateTimeCreated,
            Href = $"/project/{c.ProjectId}/register?commitment={c.Id}",
            CommitmentKind = c.Kind,
            Maturity = c.Maturity,
            QueryState = c.QueryState,
            Amount = money ? c.Amount : null,
            Currency = money ? c.Currency : null,
            AmountHidden = !money && c.Amount is not null,
            Provenance = new SearchProvenanceDto
            {
                Origin = c.SourceChannel switch
                {
                    CommitmentSource.Ingested => SearchOrigin.Thread,
                    CommitmentSource.Verbal => SearchOrigin.Spoken,
                    CommitmentSource.Meeting => SearchOrigin.Meeting,
                    _ => SearchOrigin.Register
                },
                OriginLabel = c.SourceChannel switch
                {
                    CommitmentSource.Ingested => "Read from the thread",
                    CommitmentSource.Verbal => "Agreed on a call",
                    CommitmentSource.Meeting => "Agreed in a meeting",
                    _ => "Recorded in the register"
                },
                Who = hideAuthor ? accountable : FullName(c.RecordedBy) ?? accountable,
                At = c.AgreedAt ?? c.DateTimeCreated,
                CommitmentId = c.Id,
                CommitmentTitle = c.Title
            }
        };
    }

    private async Task<List<SearchHitDto>> SearchMessagesAsync(Scope scope, CancellationToken ct)
    {
        var batches = scope.ReadableBatches.ToList();
        var hits = new List<SearchHitDto>();
        if (batches.Count == 0) return hits;

        var ids = await CandidatesAsync(
            _context.tbl_IngestedMessages.AsNoTracking().Where(m => m.BatchId != null && batches.Contains(m.BatchId) && !m.IsSystemMessage),
            t => m => m.Body != null && m.Body.Contains(t),
            m => m.Id, scope.Terms, ct);

        var idList = ids.ToList();
        var rows = await _context.tbl_IngestedMessages.AsNoTracking()
            .Include(m => m.AuthorMember).ThenInclude(a => a!.User)
            .Where(m => idList.Contains(m.Id))
            .ToListAsync(ct);

        foreach (var m in rows)
        {
            var n = scope.Terms.CountIn(m.Body);
            if (n < scope.Terms.Required) continue;

            var batch = scope.Batches[m.BatchId!];
            hits.Add(new SearchHitDto
            {
                Id = m.Id,
                Kind = SearchHitKind.Message,
                ProjectId = m.ProjectId,
                ProjectName = scope.NameOf(m.ProjectId),
                Title = scope.Terms.Snippet(m.Body, 140) ?? "",
                Snippet = scope.Terms.Snippet(m.Body, 400),
                MatchedIn = "the message",
                At = m.SentAt,
                Href = $"/project/{m.ProjectId}/history?day={m.SentAt:yyyy-MM-dd}",
                Score = Score(scope.Terms, n, false, m.SentAt),
                Provenance = new SearchProvenanceDto
                {
                    Origin = OriginOf(batch.Source),
                    OriginLabel = OriginLabelOf(batch.Source),
                    Who = AuthorName(m),
                    At = m.SentAt
                }
            });
        }
        return hits;
    }

    private async Task<List<SearchHitDto>> SearchFilesAsync(Scope scope, CancellationToken ct)
    {
        var pids = scope.ProjectIds;
        var hits = new List<SearchHitDto>();
        if (pids.Count == 0) return hits;

        // Three ways a file is found: what is written on it (OCR), its name, and
        // the caption a person gave it. The first is the one WhatsApp cannot do.
        var byText = await CandidatesAsync(
            _context.tbl_ArtifactTexts.AsNoTracking().Where(x => x.ProjectId != null && pids.Contains(x.ProjectId) && x.Status == ArtifactTextStatus.Done),
            t => x => x.Text != null && x.Text.Contains(t),
            x => x.ArtifactId!, scope.Terms, ct);

        var byName = await CandidatesAsync(
            _context.tbl_Artifacts.AsNoTracking().Where(a => a.ProjectId != null && pids.Contains(a.ProjectId)),
            t => a => a.OriginalFileName != null && a.OriginalFileName.Contains(t),
            a => a.Id, scope.Terms, ct);

        var byCaption = await CandidatesAsync(
            _context.tbl_ArtifactRefs.AsNoTracking().Where(r => r.ProjectId != null && pids.Contains(r.ProjectId)),
            t => r => r.Caption != null && r.Caption.Contains(t),
            r => r.ArtifactId!, scope.Terms, ct);

        var ids = byText.Union(byName).Union(byCaption).Where(id => !scope.ArchiveArtifacts.Contains(id)).ToList();
        if (ids.Count == 0) return hits;

        var files = await LoadFileFactsAsync(scope, ids, ct);

        foreach (var f in files.Values)
        {
            if (!f.Visible) continue;

            var text = f.Text;
            var captions = string.Join(" · ", f.Captions);
            var n = scope.Terms.CountIn(text, f.Artifact.OriginalFileName, captions);
            if (n < scope.Terms.Required) continue;

            var inText = scope.Terms.CountIn(text) > 0;
            var isImage = f.Artifact.MimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true;
            var human = f.Captions.FirstOrDefault(c => !string.Equals(c, f.Artifact.OriginalFileName, StringComparison.OrdinalIgnoreCase));

            var hit = new SearchHitDto
            {
                Id = f.Artifact.Id,
                Kind = SearchHitKind.File,
                ProjectId = f.Artifact.ProjectId,
                ProjectName = scope.NameOf(f.Artifact.ProjectId),
                Title = human ?? f.Artifact.OriginalFileName ?? "File",
                Snippet = inText ? scope.Terms.Snippet(text, 220) : human is not null ? human : null,
                MatchedIn = inText
                    ? (isImage ? "the text read from the photo" : "the text in the file")
                    : scope.Terms.CountIn(captions) > 0 ? "its caption" : "its file name",
                At = f.Message?.SentAt ?? f.Artifact.CapturedAt ?? f.Artifact.DateTimeCreated,
                ArtifactId = f.Artifact.Id,
                FileName = f.Artifact.OriginalFileName,
                MimeType = f.Artifact.MimeType,
                HasThumbnail = !string.IsNullOrEmpty(f.Artifact.ThumbnailPath),
                Href = f.Href,
                Score = Score(scope.Terms, n, false, f.Message?.SentAt ?? f.Artifact.DateTimeCreated) + (inText ? 1 : 0),
                Provenance = f.Origin
            };
            hits.Add(hit);
        }

        await AttachCommitmentsAsync(scope, hits, CommitmentLinkTarget.Artifact, ct);
        foreach (var h in hits.Where(h => h.Provenance.CommitmentId is null))
            h.Provenance.Gap = "Not tied to any commitment yet";

        return hits;
    }

    private sealed class FileFacts
    {
        public required tbl_Artifact Artifact { get; init; }
        public string? Text { get; set; }
        public List<string> Captions { get; } = new();
        public tbl_IngestedMessage? Message { get; set; }
        public bool Visible { get; set; }
        public string? Href { get; set; }
        public SearchProvenanceDto Origin { get; set; } = new();
    }

    /// <summary>
    /// Everything that decides whether this reader may see each file and where it
    /// came from. A file is visible through at least one way it reached the
    /// project that the reader may see — the same test as opening it directly.
    /// </summary>
    private async Task<Dictionary<string, FileFacts>> LoadFileFactsAsync(Scope scope, List<string> ids, CancellationToken ct)
    {
        var artifacts = await _context.tbl_Artifacts.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(ct);
        var facts = artifacts.Where(a => scope.Projects.ContainsKey(a.ProjectId ?? ""))
            .ToDictionary(a => a.Id, a => new FileFacts { Artifact = a });
        var keys = facts.Keys.ToList();

        var texts = await _context.tbl_ArtifactTexts.AsNoTracking()
            .Where(t => t.ArtifactId != null && keys.Contains(t.ArtifactId) && t.Status == ArtifactTextStatus.Done)
            .Select(t => new { t.ArtifactId, t.Text })
            .ToListAsync(ct);
        foreach (var t in texts) facts[t.ArtifactId!].Text = t.Text;

        var refs = await _context.tbl_ArtifactRefs.AsNoTracking()
            .Where(r => r.ArtifactId != null && keys.Contains(r.ArtifactId))
            .Select(r => new { r.ArtifactId, r.TargetType, r.TargetId, r.Channel, r.Caption })
            .ToListAsync(ct);

        // The message each file arrived on — carried directly, or re-joined later by its stamp.
        var carried = await _context.tbl_IngestedMessages.AsNoTracking()
            .Include(m => m.AuthorMember).ThenInclude(a => a!.User)
            .Where(m => m.ArtifactId != null && keys.Contains(m.ArtifactId))
            .ToListAsync(ct);
        var bound = await _context.tbl_MediaBindings.AsNoTracking()
            .Include(b => b.IngestedMessage).ThenInclude(m => m!.AuthorMember).ThenInclude(a => a!.User)
            .Where(b => b.ArtifactId != null && keys.Contains(b.ArtifactId))
            .ToListAsync(ct);

        var revisions = await _context.tbl_ArtifactRevisions.AsNoTracking()
            .Include(r => r.Document)
            .Where(r => r.ArtifactId != null && keys.Contains(r.ArtifactId) && r.Document != null)
            .Select(r => new { r.ArtifactId, r.Document!.Channel, r.Document.Title, r.DocumentId })
            .ToListAsync(ct);

        foreach (var f in facts.Values)
        {
            var a = f.Artifact;
            var access = scope.AccessOn(a.ProjectId);

            var messages = carried.Where(m => m.ArtifactId == a.Id)
                .Concat(bound.Where(b => b.ArtifactId == a.Id && b.IngestedMessage is not null).Select(b => b.IngestedMessage!))
                .Where(m => m.BatchId is not null && scope.ReadableBatches.Contains(m.BatchId))
                .OrderBy(m => m.SentAt)
                .ToList();
            f.Message = messages.FirstOrDefault();

            var myRefs = refs.Where(r => r.ArtifactId == a.Id).ToList();
            f.Captions.AddRange(myRefs.Select(r => r.Caption).OfType<string>().Where(c => c.Length > 0).Distinct());

            var docs = revisions.Where(r => r.ArtifactId == a.Id).ToList();
            var visibleDoc = docs.FirstOrDefault(d => access.CanSeeDocuments && (access.CanSeeSiteLog || d.Channel == Channel.Client));
            var visibleCapture = myRefs.FirstOrDefault(r =>
                (r.TargetType is ArtifactTargetType.ProgressUpdate or ArtifactTargetType.Brief or ArtifactTargetType.Commitment)
                && (access.CanSeeSiteLog || r.Channel == Channel.Client));
            var visibleReceipt = myRefs.Any(r => r.TargetType == ArtifactTargetType.Receipt && access.CanSeeMoney
                                                 && (access.CanSeeSiteLog || r.Channel == Channel.Client));
            var visibleIngest = myRefs.Any(r => r.TargetType == ArtifactTargetType.IngestedMessage
                                                && access.Seat == ProjectSeat.Principal
                                                && (access.CanSeeSiteLog || r.Channel == Channel.Client));

            f.Visible = a.UploadedById == scope.UserId || f.Message is not null || visibleDoc is not null
                        || visibleCapture is not null || visibleReceipt || visibleIngest;

            if (f.Message is { } m)
            {
                var source = scope.Batches.TryGetValue(m.BatchId!, out var b) ? b.Source : IngestSourceType.WhatsAppExport;
                f.Origin = new SearchProvenanceDto
                {
                    Origin = OriginOf(source),
                    OriginLabel = OriginLabelOf(source),
                    Who = AuthorName(m),
                    At = m.SentAt
                };
                f.Href = $"/project/{a.ProjectId}/history?day={m.SentAt:yyyy-MM-dd}";
            }
            else if (visibleDoc is not null)
            {
                f.Origin = new SearchProvenanceDto { Origin = SearchOrigin.Document, OriginLabel = $"Drawing register — {visibleDoc.Title}", At = a.DateTimeCreated };
                f.Href = $"/project/{a.ProjectId}/documents";
            }
            else if (visibleCapture is not null && visibleCapture.TargetType == ArtifactTargetType.ProgressUpdate)
            {
                f.Origin = new SearchProvenanceDto
                {
                    Origin = SearchOrigin.Capture,
                    OriginLabel = "Captured on site",
                    Who = access.CanSeeSiteLog ? null : scope.MediatorName.GetValueOrDefault(a.ProjectId ?? ""),
                    At = a.CapturedAt ?? a.DateTimeCreated
                };
                f.Href = $"/project/{a.ProjectId}/entry/{visibleCapture.TargetId}";
            }
            else
            {
                f.Origin = new SearchProvenanceDto { Origin = SearchOrigin.Shared, OriginLabel = "Added to the project", At = a.CapturedAt ?? a.DateTimeCreated };
                f.Href = null;
            }
        }

        return facts;
    }

    private async Task<List<SearchHitDto>> SearchDiaryAsync(Scope scope, CancellationToken ct)
    {
        var pids = scope.ProjectIds;
        var hits = new List<SearchHitDto>();
        if (pids.Count == 0) return hits;

        var ids = await CandidatesAsync(
            _context.tbl_ProgressUpdates.AsNoTracking().Where(u => u.ProjectId != null && pids.Contains(u.ProjectId)),
            t => u => u.Description != null && u.Description.Contains(t),
            u => u.Id, scope.Terms, ct);

        var idList = ids.ToList();
        var rows = await _context.tbl_ProgressUpdates.AsNoTracking()
            .Include(u => u.Stage)
            .Include(u => u.CreatedBy)
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);

        foreach (var u in rows)
        {
            var access = scope.AccessOn(u.ProjectId);

            // The Diary is the delivery side's; the client side reads only what a mediator exposed.
            if (!(access.CanSeeSiteLog || (u.Channel == Channel.Client && access.CanSeeBrief))) continue;

            var n = scope.Terms.CountIn(u.Description);
            if (n < scope.Terms.Required) continue;

            hits.Add(new SearchHitDto
            {
                Id = u.Id,
                Kind = SearchHitKind.DiaryEntry,
                ProjectId = u.ProjectId,
                ProjectName = scope.NameOf(u.ProjectId),
                StageId = u.StageId,
                StageName = u.Stage?.StageName,
                StagePhase = u.Stage?.Phase,
                Title = scope.Terms.Snippet(u.Description, 140) ?? "",
                Snippet = scope.Terms.Snippet(u.Description, 300),
                MatchedIn = "the entry",
                At = u.DateTimeCreated,
                Href = $"/project/{u.ProjectId}/entry/{u.Id}",
                Score = Score(scope.Terms, n, false, u.DateTimeCreated),
                Provenance = new SearchProvenanceDto
                {
                    Origin = SearchOrigin.Capture,
                    OriginLabel = u.Channel == Channel.Client ? "Site Diary, shared with the client" : "Site Diary",
                    Who = access.CanSeeSiteLog ? FullName(u.CreatedBy) : scope.MediatorName.GetValueOrDefault(u.ProjectId ?? ""),
                    At = u.DateTimeCreated
                }
            });
        }

        await AttachCommitmentsAsync(scope, hits, CommitmentLinkTarget.ProgressUpdate, ct);
        return hits;
    }

    private async Task<List<SearchHitDto>> SearchStagesAndProjectsAsync(Scope scope, CancellationToken ct)
    {
        var hits = new List<SearchHitDto>();
        foreach (var (pid, (p, _)) in scope.Projects)
        {
            var n = scope.Terms.CountIn(p.ProjectName, p.Description);
            if (n < scope.Terms.Required) continue;
            hits.Add(new SearchHitDto
            {
                Id = pid, Kind = SearchHitKind.Project, ProjectId = pid, ProjectName = p.ProjectName,
                Title = p.ProjectName ?? "Project", Snippet = scope.Terms.Snippet(p.Description),
                MatchedIn = scope.Terms.CountIn(p.ProjectName) > 0 ? "its name" : "its description",
                At = p.DateTimeCreated, Href = $"/project/{pid}", Score = Score(scope.Terms, n, true, null),
                Provenance = new SearchProvenanceDto { Origin = SearchOrigin.Project, OriginLabel = p.ParentProjectId is null ? "Project" : "Sub-project" }
            });
        }

        var pids = scope.ProjectIds;
        if (pids.Count == 0) return hits;

        var ids = await CandidatesAsync(
            _context.tbl_Stages.AsNoTracking().Where(s => s.ProjectId != null && pids.Contains(s.ProjectId)),
            t => s => (s.StageName != null && s.StageName.Contains(t)) || (s.Description != null && s.Description.Contains(t)),
            s => s.Id, scope.Terms, ct);
        var idList = ids.ToList();
        var stages = await _context.tbl_Stages.AsNoTracking().Where(s => idList.Contains(s.Id)).ToListAsync(ct);

        foreach (var s in stages)
        {
            var n = scope.Terms.CountIn(s.StageName, s.Description);
            if (n < scope.Terms.Required) continue;
            hits.Add(new SearchHitDto
            {
                Id = s.Id, Kind = SearchHitKind.Stage, ProjectId = s.ProjectId, ProjectName = scope.NameOf(s.ProjectId),
                StageId = s.Id, StageName = s.StageName, StagePhase = s.Phase,
                Title = s.StageName ?? "Stage", Snippet = scope.Terms.Snippet(s.Description),
                MatchedIn = scope.Terms.CountIn(s.StageName) > 0 ? "its name" : "its description",
                At = s.StartDate ?? s.DateTimeCreated, Href = $"/project/{s.ProjectId}/stage/{s.Id}",
                Score = Score(scope.Terms, n, scope.Terms.CountIn(s.StageName) > 0, null),
                Provenance = new SearchProvenanceDto { Origin = SearchOrigin.Project, OriginLabel = $"Stage of {scope.NameOf(s.ProjectId)}" }
            });
        }
        return hits;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Grouping and provenance
    // ═════════════════════════════════════════════════════════════════════

    private async Task FoldMessagesIntoCommitmentsAsync(
        Scope scope, List<SearchHitDto> messages, Dictionary<string, SearchHitDto> commitments, CancellationToken ct)
    {
        if (messages.Count == 0) return;
        var registerPids = scope.Projects.Where(p => p.Value.Access.CanSeeRegister).Select(p => p.Key).ToList();
        if (registerPids.Count == 0) return;

        var messageIds = messages.Select(m => m.Id).ToList();

        var direct = await _context.tbl_Commitments.AsNoTracking()
            .Where(c => c.IngestedMessageId != null && messageIds.Contains(c.IngestedMessageId)
                        && c.ProjectId != null && registerPids.Contains(c.ProjectId))
            .Select(c => new { c.Id, MessageId = c.IngestedMessageId!, c.SupersededById, c.SupersededAt })
            .ToListAsync(ct);
        var linked = await _context.tbl_CommitmentLinks.AsNoTracking()
            .Where(l => l.TargetType == CommitmentLinkTarget.IngestedMessage && l.TargetId != null && messageIds.Contains(l.TargetId)
                        && l.ProjectId != null && registerPids.Contains(l.ProjectId) && l.Relation == CommitmentLinkRelation.Source)
            .Select(l => new { Id = l.CommitmentId!, MessageId = l.TargetId! })
            .ToListAsync(ct);

        var pairs = direct.Select(d => (d.Id, d.MessageId)).Concat(linked.Select(l => (l.Id, l.MessageId))).Distinct().ToList();
        if (pairs.Count == 0) return;

        // A restated commitment is found through its first statement, but the answer is the current one.
        var heads = await HeadsAsync(pairs.Select(p => p.Id).Distinct().ToList(), ct);
        var toLoad = heads.Values.Where(h => !commitments.ContainsKey(h)).Distinct().ToList();
        foreach (var c in await LoadCommitmentsAsync(toLoad, ct))
        {
            var hit = CommitmentHit(scope, c);
            hit.Score = 0;
            commitments[c.Id] = hit;
        }

        foreach (var (cid, mid) in pairs)
        {
            if (!heads.TryGetValue(cid, out var head) || !commitments.TryGetValue(head, out var hit)) continue;
            var message = messages.FirstOrDefault(m => m.Id == mid);
            if (message is null) continue;

            messages.Remove(message);
            if (hit.MatchedIn is null || hit.Score < message.Score)
            {
                hit.MatchedIn = "the message it was agreed in";
                hit.Snippet = $"{message.Provenance.Who}: {message.Snippet}";
            }
            hit.Score = Math.Max(hit.Score, message.Score + (scope.Terms.AsksForAgreement ? 3 : 1));
        }
    }

    /// <summary>Commitment id → the id of its current statement.</summary>
    private async Task<Dictionary<string, string>> HeadsAsync(List<string> ids, CancellationToken ct)
    {
        var result = ids.ToDictionary(i => i, i => i);
        var frontier = ids;
        for (var guard = 0; guard < 20 && frontier.Count > 0; guard++)
        {
            var next = await _context.tbl_Commitments.AsNoTracking()
                .Where(c => frontier.Contains(c.Id) && c.SupersededById != null)
                .Select(c => new { c.Id, Next = c.SupersededById! })
                .ToListAsync(ct);
            if (next.Count == 0) break;
            foreach (var n in next)
                foreach (var k in result.Where(r => r.Value == n.Id).Select(r => r.Key).ToList())
                    result[k] = n.Next;
            frontier = next.Select(n => n.Next).ToList();
        }
        return result;
    }

    /// <summary>
    /// For photos and diary entries: the commitment this proves, bills or
    /// settles, and that commitment's strip. Only commitments the reader's seat
    /// has a register for are named — a link is never a way round the register.
    /// </summary>
    private async Task AttachCommitmentsAsync(Scope scope, List<SearchHitDto> hits, CommitmentLinkTarget targetType, CancellationToken ct)
    {
        if (hits.Count == 0) return;
        var registerPids = scope.Projects.Where(p => p.Value.Access.CanSeeRegister).Select(p => p.Key).ToList();
        if (registerPids.Count == 0) return;

        var targetIds = hits.Select(h => h.Id).ToList();
        var links = await _context.tbl_CommitmentLinks.AsNoTracking()
            .Where(l => l.TargetType == targetType && l.TargetId != null && targetIds.Contains(l.TargetId)
                        && l.ProjectId != null && registerPids.Contains(l.ProjectId))
            .ToListAsync(ct);
        if (links.Count == 0) return;

        var heads = await HeadsAsync(links.Select(l => l.CommitmentId!).Distinct().ToList(), ct);
        var commitments = (await LoadCommitmentsAsync(heads.Values.Distinct(), ct)).ToDictionary(c => c.Id);
        var strips = await StepsAsync(scope, commitments.Values.ToList(), ct);

        foreach (var hit in hits)
        {
            // The strongest tie first: a receipt that bills a commitment says more than one that merely relates.
            var link = links.Where(l => l.TargetId == hit.Id)
                .OrderBy(l => l.Relation switch
                {
                    CommitmentLinkRelation.Invoice => 0,
                    CommitmentLinkRelation.Clears => 1,
                    CommitmentLinkRelation.Evidence => 2,
                    CommitmentLinkRelation.Source => 3,
                    _ => 4
                })
                .FirstOrDefault();
            if (link is null || !heads.TryGetValue(link.CommitmentId!, out var head) || !commitments.TryGetValue(head, out var c)) continue;

            hit.Provenance.CommitmentId = c.Id;
            hit.Provenance.CommitmentTitle = c.Title;
            hit.Provenance.Role = link.Relation switch
            {
                CommitmentLinkRelation.Invoice => "Invoice for",
                CommitmentLinkRelation.Clears => "Settles",
                CommitmentLinkRelation.Evidence => "Evidence for",
                CommitmentLinkRelation.Source => "Where it was agreed:",
                _ => "Related to"
            };
            var own = link.Relation switch
            {
                CommitmentLinkRelation.Invoice => ProvenanceStep.Invoiced,
                CommitmentLinkRelation.Clears => ProvenanceStep.Cleared,
                CommitmentLinkRelation.Evidence => ProvenanceStep.Evidence,
                CommitmentLinkRelation.Source => ProvenanceStep.Agreed,
                _ => (ProvenanceStep?)null
            };
            hit.Provenance.Steps = strips[c.Id].Select(s => new ProvenanceStepDto
            {
                Step = s.Step, Label = s.Label, Reached = s.Reached, At = s.At, IsThis = own == s.Step
            }).ToList();
        }
    }

    private async Task ProvenanceForCommitmentsAsync(Scope scope, Dictionary<string, SearchHitDto> hits, CancellationToken ct)
    {
        if (hits.Count == 0) return;
        var ids = hits.Keys.ToList();
        var rows = await _context.tbl_Commitments.AsNoTracking().Where(c => ids.Contains(c.Id)).ToListAsync(ct);
        var strips = await StepsAsync(scope, rows, ct);
        foreach (var (id, hit) in hits)
            if (strips.TryGetValue(id, out var steps)) hit.Provenance.Steps = steps;
    }

    /// <summary>
    /// agreed → evidence → invoiced → cleared → queried → resolved, from the
    /// commitment's own dates and the links the reader may see. A link to a
    /// crew-only photo does not light "evidence" for the client side: the strip
    /// must not announce what the reader cannot open.
    /// </summary>
    private async Task<Dictionary<string, List<ProvenanceStepDto>>> StepsAsync(Scope scope, List<tbl_Commitment> commitments, CancellationToken ct)
    {
        var ids = commitments.Select(c => c.Id).ToList();
        var links = await _context.tbl_CommitmentLinks.AsNoTracking()
            .Where(l => l.CommitmentId != null && ids.Contains(l.CommitmentId))
            .ToListAsync(ct);

        var visible = await VisibleLinksAsync(scope, links, ct);

        var result = new Dictionary<string, List<ProvenanceStepDto>>();
        foreach (var c in commitments)
        {
            var mine = visible.Where(l => l.CommitmentId == c.Id).ToList();
            DateTime? First(Func<tbl_CommitmentLink, bool> p) => mine.Where(p).Select(l => l.DateTimeCreated).Min();

            var evidenceAt = First(l => l.Relation == CommitmentLinkRelation.Evidence);
            var invoiceAt = First(l => l.Relation == CommitmentLinkRelation.Invoice || l.TargetType == CommitmentLinkTarget.Claim);
            var clearsAt = First(l => l.Relation == CommitmentLinkRelation.Clears);
            var queried = c.QueryState is CommitmentQueryState.QueryRaised or CommitmentQueryState.Resolved || c.DisputedAt is not null;

            result[c.Id] = new List<ProvenanceStepDto>
            {
                new()
                {
                    Step = ProvenanceStep.Agreed,
                    Label = c.Maturity switch
                    {
                        CommitmentMaturity.Idea => "Idea",
                        CommitmentMaturity.InDiscussion => "In discussion",
                        _ => "Agreed"
                    },
                    Reached = c.Maturity >= CommitmentMaturity.Agreed,
                    At = c.AgreedAt
                },
                new()
                {
                    Step = ProvenanceStep.Evidence, Label = "Evidence",
                    Reached = evidenceAt is not null || c.Maturity >= CommitmentMaturity.Delivered,
                    At = evidenceAt ?? c.DeliveredAt
                },
                new() { Step = ProvenanceStep.Invoiced, Label = "Invoiced", Reached = invoiceAt is not null, At = invoiceAt },
                new() { Step = ProvenanceStep.Cleared, Label = "Cleared", Reached = c.ClearedAt is not null || clearsAt is not null, At = c.ClearedAt ?? clearsAt },
                new() { Step = ProvenanceStep.Queried, Label = "Queried", Reached = queried, At = c.DisputedAt },
                new() { Step = ProvenanceStep.Resolved, Label = "Resolved", Reached = c.ResolvedAt is not null, At = c.ResolvedAt }
            };
        }
        return result;
    }

    private async Task<List<tbl_CommitmentLink>> VisibleLinksAsync(Scope scope, List<tbl_CommitmentLink> links, CancellationToken ct)
    {
        if (links.Count == 0) return links;

        var artifactIds = links.Where(l => l.TargetType == CommitmentLinkTarget.Artifact).Select(l => l.TargetId!).Distinct().ToList();
        var artifactFacts = artifactIds.Count == 0 ? new Dictionary<string, FileFacts>() : await LoadFileFactsAsync(scope, artifactIds, ct);

        var messageIds = links.Where(l => l.TargetType == CommitmentLinkTarget.IngestedMessage).Select(l => l.TargetId!).Distinct().ToList();
        var messageBatches = await _context.tbl_IngestedMessages.AsNoTracking()
            .Where(m => messageIds.Contains(m.Id)).Select(m => new { m.Id, m.BatchId }).ToDictionaryAsync(m => m.Id, m => m.BatchId, ct);

        var updateIds = links.Where(l => l.TargetType == CommitmentLinkTarget.ProgressUpdate).Select(l => l.TargetId!).Distinct().ToList();
        var updates = await _context.tbl_ProgressUpdates.AsNoTracking()
            .Where(u => updateIds.Contains(u.Id)).Select(u => new { u.Id, u.Channel }).ToDictionaryAsync(u => u.Id, u => u.Channel, ct);

        var flagIds = links.Where(l => l.TargetType == CommitmentLinkTarget.Flag).Select(l => l.TargetId!).Distinct().ToList();
        var flags = await _context.tbl_Flags.AsNoTracking()
            .Where(f => flagIds.Contains(f.Id)).Select(f => new { f.Id, f.Channel }).ToDictionaryAsync(f => f.Id, f => f.Channel, ct);

        var docIds = links.Where(l => l.TargetType == CommitmentLinkTarget.Document).Select(l => l.TargetId!).Distinct().ToList();
        var docs = await _context.tbl_Documents.AsNoTracking()
            .Where(d => docIds.Contains(d.Id)).Select(d => new { d.Id, d.Channel }).ToDictionaryAsync(d => d.Id, d => d.Channel, ct);

        return links.Where(l =>
        {
            var a = scope.AccessOn(l.ProjectId);
            var id = l.TargetId ?? "";
            return l.TargetType switch
            {
                CommitmentLinkTarget.Commitment => a.CanSeeRegister,
                CommitmentLinkTarget.Artifact => artifactFacts.TryGetValue(id, out var f) && f.Visible,
                CommitmentLinkTarget.IngestedMessage => messageBatches.TryGetValue(id, out var b) && b is not null && scope.ReadableBatches.Contains(b),
                CommitmentLinkTarget.ProgressUpdate => updates.TryGetValue(id, out var ch) && (a.CanSeeSiteLog || ch == Channel.Client),
                CommitmentLinkTarget.Flag => flags.TryGetValue(id, out var fc) && (a.CanSeeSiteLog || fc == Channel.Client),
                CommitmentLinkTarget.Document => docs.TryGetValue(id, out var dc) && a.CanSeeDocuments && (a.CanSeeSiteLog || dc == Channel.Client),
                CommitmentLinkTarget.FundingEntry or CommitmentLinkTarget.Claim or CommitmentLinkTarget.Variation => a.CanSeeMoney,
                _ => false
            };
        }).ToList();
    }

    // ═════════════════════════════════════════════════════════════════════
    // Where it could not look
    // ═════════════════════════════════════════════════════════════════════

    private async Task<int> FilesAwaitingTextAsync(Scope scope, CancellationToken ct)
    {
        var pids = scope.ProjectIds;
        if (pids.Count == 0) return 0;
        var archives = scope.ArchiveArtifacts.ToList();

        return await _context.tbl_Artifacts.AsNoTracking()
            .Where(a => a.ProjectId != null && pids.Contains(a.ProjectId) && !archives.Contains(a.Id))
            .Where(a => !_context.tbl_ArtifactTexts.Any(t => t.ArtifactId == a.Id
                        && t.Status != ArtifactTextStatus.Pending
                        && t.Status != ArtifactTextStatus.EngineUnavailable
                        && t.Status != ArtifactTextStatus.Failed))
            .CountAsync(ct);
    }

    private async Task<bool> FullTextInstalledAsync(CancellationToken ct)
    {
        if (_fullTextInstalled is { } known) return known;
        try
        {
            var v = await _context.Database
                .SqlQueryRaw<int>("SELECT CAST(ISNULL(SERVERPROPERTY('IsFullTextInstalled'), 0) AS int) AS [Value]")
                .FirstAsync(ct);
            _fullTextInstalled = v == 1;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not ask the server whether full-text search is installed");
            _fullTextInstalled = false;
        }
        return _fullTextInstalled.Value;
    }

    // ═════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════

    /// <summary>Coverage first, then a word in the title, then recency as a tiebreak.</summary>
    private static double Score(SearchTerms terms, int matched, bool inTitle, DateTime? at)
    {
        var coverage = terms.Terms.Count == 0 ? 0 : 10.0 * matched / terms.Terms.Count;
        var recency = at is { } d ? Math.Clamp(1 - (DateTime.UtcNow - d).TotalDays / 365.0, 0, 1) : 0;
        return coverage + (inTitle ? 3 : 0) + recency;
    }

    private static SearchOrigin OriginOf(IngestSourceType s) => s switch
    {
        IngestSourceType.ShareSheet => SearchOrigin.Shared,
        IngestSourceType.Email => SearchOrigin.Email,
        _ => SearchOrigin.Thread
    };

    private static string OriginLabelOf(IngestSourceType s) => s switch
    {
        IngestSourceType.ShareSheet => "Shared from a phone",
        IngestSourceType.Email => "Forwarded by email",
        IngestSourceType.Manual => "Typed in",
        _ => "From the thread"
    };

    /// <summary>The mapped member when there is one, else the name exactly as the export gave it.</summary>
    private static string? AuthorName(tbl_IngestedMessage m) =>
        m.AuthorMember is not null ? MemberName(m.AuthorMember) : string.IsNullOrWhiteSpace(m.ExternalAuthor) ? null : m.ExternalAuthor;

    private static string? FullName(AppUser? u) => u is null ? null : $"{u.FirstName} {u.LastName}".Trim();

    private static string MemberName(tbl_ProjectMember m) => FullName(m.User) ?? m.PartyName ?? m.Title ?? "Unnamed";

    private static ServiceResult<SearchResultDto> Fail(Exception e) => ServiceResult<SearchResultDto>.Failure(e);
}
