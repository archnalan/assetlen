using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using assetlen.Shared.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace assetlen.Service.DataAccess;

public partial class AssetlenDbContext : IdentityDbContext<AppUser>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly string _tenantId;
    private readonly bool _isSuperAdmin;

    public AssetlenDbContext(DbContextOptions<AssetlenDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _tenantId = tenantProvider.GetTenantId();
        _isSuperAdmin = tenantProvider.IsSuperAdmin();
    }

    // ─── Platform ──────────────────────────────────────────────
    public virtual DbSet<tbl_Tenant> tbl_Tenants { get; set; }
    public virtual DbSet<tbl_Configuration> tbl_Configurations { get; set; }
    public virtual DbSet<tbl_Log> tbl_Logs { get; set; }
    public virtual DbSet<tbl_RoleValue> tbl_RoleValues { get; set; }
    public virtual DbSet<tbl_SyncLog> tbl_SyncLogs { get; set; }
    public DbSet<tbl_RefreshToken> RefreshTokens { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<tbl_SubscriptionRequest> tbl_SubscriptionRequests { get; set; }
    public DbSet<tbl_SubscriptionSeat> tbl_SubscriptionSeats { get; set; }
    public virtual DbSet<tbl_EmployeeApproval> tbl_EmployeeApprovals { get; set; }

    // ─── Projects + Site Diary (ASSETLEN core) ───────────────────
    public virtual DbSet<tbl_Project> tbl_Projects_RS { get; set; }
    public virtual DbSet<tbl_Stage> tbl_Stages { get; set; }
    public virtual DbSet<tbl_FundingEntry> tbl_FundingEntries { get; set; }
    public virtual DbSet<tbl_ProgressUpdate> tbl_ProgressUpdates { get; set; }
    public virtual DbSet<tbl_ProgressImage> tbl_ProgressImages { get; set; }
    public virtual DbSet<tbl_ProgressComment> tbl_ProgressComments { get; set; }
    public virtual DbSet<tbl_ProjectSubscription> tbl_ProjectSubscriptions { get; set; }
    public virtual DbSet<tbl_Flag> tbl_Flags { get; set; }
    public virtual DbSet<tbl_ProjectMember> tbl_ProjectMembers { get; set; }
    public virtual DbSet<tbl_ProjectPreference> tbl_ProjectPreferences { get; set; }
    public virtual DbSet<tbl_BudgetLineItem> tbl_BudgetLineItems { get; set; }
    public virtual DbSet<tbl_Receipt> tbl_Receipts { get; set; }

    /// <summary>
    /// One human, many accounts (assetlen.md §10.2). **Not** tenant-scoped —
    /// see the entity remarks.
    /// </summary>
    public virtual DbSet<tbl_TenantMembership> tbl_TenantMemberships { get; set; }

    // ─── Artifact store (P2 — assetlen.md Law 2) ───────────────
    // One canonical file per hash; every use is a ref, and the ref carries the
    // Client/Crew exposure. Documents pin a current revision over an
    // append-only revision chain.
    public virtual DbSet<tbl_Artifact> tbl_Artifacts { get; set; }
    public virtual DbSet<tbl_ArtifactRef> tbl_ArtifactRefs { get; set; }
    public virtual DbSet<tbl_Document> tbl_Documents { get; set; }
    public virtual DbSet<tbl_ArtifactRevision> tbl_ArtifactRevisions { get; set; }

    // ─── Ingest — the front door (P3 — assetlen.md D3) ─────────
    // Raw forwarded material, landed verbatim. Extraction (P5) reads
    // tbl_IngestedMessages; nothing else writes them.
    public virtual DbSet<tbl_IngestBatch> tbl_IngestBatches { get; set; }
    public virtual DbSet<tbl_IngestedMessage> tbl_IngestedMessages { get; set; }

    // ─── The commitment model + money ledger (P4 — assetlen.md §3, §6) ───
    public virtual DbSet<tbl_Deliverable> tbl_Deliverables { get; set; }
    public virtual DbSet<tbl_DeliverableEvent> tbl_DeliverableEvents { get; set; }
    public virtual DbSet<tbl_PlanWait> tbl_PlanWaits { get; set; }
    public virtual DbSet<tbl_WorkSchedule> tbl_WorkSchedules { get; set; }
    public virtual DbSet<tbl_Commitment> tbl_Commitments { get; set; }
    public virtual DbSet<tbl_CommitmentLink> tbl_CommitmentLinks { get; set; }
    public virtual DbSet<tbl_Variation> tbl_Variations { get; set; }
    public virtual DbSet<tbl_StageClaim> tbl_StageClaims { get; set; }

    // ─── Extraction — pile into register (P5 — assetlen.md Law 3) ───
    // Proposals wait for a person; readings and OCR text are observations with
    // a source; bindings re-join loose media without editing the raw record.
    public virtual DbSet<tbl_ArtifactText> tbl_ArtifactTexts { get; set; }
    public virtual DbSet<tbl_ExtractionRun> tbl_ExtractionRuns { get; set; }
    public virtual DbSet<tbl_ExtractionProposal> tbl_ExtractionProposals { get; set; }
    public virtual DbSet<tbl_ProgressReading> tbl_ProgressReadings { get; set; }
    public virtual DbSet<tbl_MediaBinding> tbl_MediaBindings { get; set; }

    // ─── The works report (works-report.md) — issued snapshots and video posters ───
    public virtual DbSet<tbl_WorksReport> tbl_WorksReports { get; set; }
    public virtual DbSet<tbl_ArtifactPoster> tbl_ArtifactPosters { get; set; }
    public virtual DbSet<tbl_Annotation> tbl_Annotations { get; set; }
    public virtual DbSet<tbl_CommitmentEstimate> tbl_CommitmentEstimates { get; set; }
    public virtual DbSet<tbl_BriefPublication> tbl_BriefPublications { get; set; }
    public virtual DbSet<tbl_ClaimEvidence> tbl_ClaimEvidence { get; set; }
    public virtual DbSet<tbl_PushSubscription> tbl_PushSubscriptions { get; set; }
    public virtual DbSet<tbl_PushDelivery> tbl_PushDeliveries { get; set; }

    /// <summary>
    /// The one tenancy rule, applied per entity:
    ///   (SuperAdmin OR same tenant OR unowned OR Public) AND not Protected AND not soft-deleted.
    /// </summary>
    /// <remarks>
    /// This used to be 52 hand-copied lambdas. Adding an entity without its
    /// filter leaked rows across tenants, so it is now a single call — if you
    /// add a <see cref="DbSet{TEntity}"/> above, add its <c>TenantScoped</c>
    /// line below unless the table is deliberately global (tenants, seats).
    /// </remarks>
    private void TenantScoped<TEntity>(ModelBuilder modelBuilder) where TEntity : class, IBaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register Identity defaults first — .NET 10 added IdentityPasskeyData
        // and similar passkey/WebAuthn types that need their key config from
        // the base. Custom configurations below may override individual bits.
        base.OnModelCreating(modelBuilder);

        // ─── Multi-tenant + Access query filters ──────────────────────
        TenantScoped<AppUser>(modelBuilder);
        TenantScoped<tbl_Configuration>(modelBuilder);
        TenantScoped<tbl_Log>(modelBuilder);
        TenantScoped<tbl_RoleValue>(modelBuilder);
        TenantScoped<tbl_SyncLog>(modelBuilder);
        TenantScoped<tbl_RefreshToken>(modelBuilder);
        TenantScoped<tbl_EmployeeApproval>(modelBuilder);

        TenantScoped<tbl_Project>(modelBuilder);
        TenantScoped<tbl_Stage>(modelBuilder);
        TenantScoped<tbl_FundingEntry>(modelBuilder);
        TenantScoped<tbl_ProgressUpdate>(modelBuilder);
        TenantScoped<tbl_ProgressImage>(modelBuilder);
        TenantScoped<tbl_ProgressComment>(modelBuilder);
        TenantScoped<tbl_ProjectSubscription>(modelBuilder);
        TenantScoped<tbl_Flag>(modelBuilder);
        TenantScoped<tbl_ProjectMember>(modelBuilder);
        TenantScoped<tbl_ProjectPreference>(modelBuilder);
        TenantScoped<tbl_BudgetLineItem>(modelBuilder);
        TenantScoped<tbl_Receipt>(modelBuilder);
        TenantScoped<tbl_Artifact>(modelBuilder);
        TenantScoped<tbl_ArtifactRef>(modelBuilder);
        TenantScoped<tbl_Document>(modelBuilder);
        TenantScoped<tbl_ArtifactRevision>(modelBuilder);
        TenantScoped<tbl_IngestBatch>(modelBuilder);
        TenantScoped<tbl_IngestedMessage>(modelBuilder);
        TenantScoped<tbl_Deliverable>(modelBuilder);
        TenantScoped<tbl_DeliverableEvent>(modelBuilder);
        TenantScoped<tbl_PlanWait>(modelBuilder);
        TenantScoped<tbl_WorkSchedule>(modelBuilder);
        TenantScoped<tbl_Commitment>(modelBuilder);
        TenantScoped<tbl_CommitmentLink>(modelBuilder);
        TenantScoped<tbl_Variation>(modelBuilder);
        TenantScoped<tbl_StageClaim>(modelBuilder);
        TenantScoped<tbl_ArtifactText>(modelBuilder);
        TenantScoped<tbl_ExtractionRun>(modelBuilder);
        TenantScoped<tbl_ExtractionProposal>(modelBuilder);
        TenantScoped<tbl_ProgressReading>(modelBuilder);
        TenantScoped<tbl_MediaBinding>(modelBuilder);
        TenantScoped<tbl_WorksReport>(modelBuilder);
        TenantScoped<tbl_ArtifactPoster>(modelBuilder);
        TenantScoped<tbl_Annotation>(modelBuilder);
        TenantScoped<tbl_CommitmentEstimate>(modelBuilder);
        TenantScoped<tbl_BriefPublication>(modelBuilder);
        TenantScoped<tbl_ClaimEvidence>(modelBuilder);
        TenantScoped<tbl_PushSubscription>(modelBuilder);
        TenantScoped<tbl_PushDelivery>(modelBuilder);

        // Channel-based (Client/Crew) visibility is enforced at the service
        // layer, not here: it depends on the caller's *per-project* side, which
        // a DbContext-level filter cannot see. ArtifactDAL is the choke point —
        // it resolves ProjectAccess once and filters refs on
        // ProjectAccess.CanSeeSiteLog. Do not add a channel query filter here
        // and assume it covers the surface; it would silently miss the
        // mediator, who is client-side yet entitled to the whole Site Diary.

        // ─── Projects + Site Diary relationships ─────────────────────
        modelBuilder.Entity<tbl_Project>(entity =>
        {
            entity.ToTable("tbl_Projects_RS");
            entity.HasIndex(e => e.InvestorId).HasDatabaseName("IX_Project_InvestorId");
            entity.HasIndex(e => e.ProjectManagerId).HasDatabaseName("IX_Project_ProjectManagerId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Project_Status");
            entity.HasIndex(e => e.ParentProjectId).HasDatabaseName("IX_Project_ParentProjectId");

            // Every list of live projects filters on this, and the bin sweep
            // asks for the handful whose thirty days are up.
            entity.HasIndex(e => e.ArchivedAt).HasDatabaseName("IX_Project_ArchivedAt");

            // The developer's account owns the project. Every child row is
            // stamped from here — see ResolveOwningTenantId.
            entity.HasIndex(e => e.OwnerTenantId).HasDatabaseName("IX_Project_OwnerTenantId");

            // Billing is per project by size; this index backs the tier rollup.
            entity.HasIndex(e => e.SizeTier).HasDatabaseName("IX_Project_SizeTier");

            entity.Property(e => e.TotalBudget).HasColumnType("decimal(18,4)");
            entity.Property(e => e.FloorAreaSqm).HasColumnType("decimal(12,2)");
            entity.HasOne(e => e.Investor).WithMany().HasForeignKey(e => e.InvestorId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProjectManager).WithMany().HasForeignKey(e => e.ProjectManagerId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            // Self-ref for one-level Sub-project nesting. NoAction on delete —
            // the service layer detaches Sub-projects before deleting a parent.
            entity.HasOne(e => e.ParentProject).WithMany(p => p.SubProjects).HasForeignKey(e => e.ParentProjectId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_Stage>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Stage_ProjectId");
            entity.Property(e => e.BudgetAmount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)");
            entity.HasOne(e => e.Project).WithMany(p => p.Stages).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_FundingEntry>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_FundingEntry_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_FundingEntry_StageId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_FundingEntry_Status");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.DeclaredAmount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ReceivedAmount).HasColumnType("decimal(18,4)");

            // Wider than the money columns on purpose: a rate the other way round
            // (USD per UGX) is 0.00026, and rounding that to four places turns a
            // release into a different number than the one the funder sent.
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,8)");

            entity.HasOne(e => e.Project).WithMany(p => p.FundingEntries).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Stage).WithMany(s => s.FundingEntries).HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.PaidBy).WithMany().HasForeignKey(e => e.PaidById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ConfirmedBy).WithMany().HasForeignKey(e => e.ConfirmedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ProgressUpdate>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ProgressUpdate_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_ProgressUpdate_StageId");
            entity.HasIndex(e => e.DateTimeCreated).HasDatabaseName("IX_ProgressUpdate_CreatedAt");
            entity.Property(e => e.CompletionPercentage).HasColumnType("decimal(5,2)");
            entity.HasOne(e => e.Project).WithMany(p => p.ProgressUpdates).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Stage).WithMany(s => s.ProgressUpdates).HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ProgressImage>(entity =>
        {
            entity.HasIndex(e => e.ProgressUpdateId).HasDatabaseName("IX_ProgressImage_UpdateId");
            entity.HasOne(e => e.ProgressUpdate).WithMany(u => u.Images).HasForeignKey(e => e.ProgressUpdateId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_ProgressComment>(entity =>
        {
            entity.HasIndex(e => e.ProgressUpdateId).HasDatabaseName("IX_ProgressComment_UpdateId");
            entity.HasIndex(e => e.ProgressImageId).HasDatabaseName("IX_ProgressComment_ImageId");
            entity.HasOne(e => e.ProgressUpdate).WithMany(u => u.Comments).HasForeignKey(e => e.ProgressUpdateId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProgressImage).WithMany(i => i.Comments).HasForeignKey(e => e.ProgressImageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Author).WithMany().HasForeignKey(e => e.AuthorId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ParentComment).WithMany(c => c.Replies).HasForeignKey(e => e.ParentCommentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ProjectSubscription>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ProjectSub_ProjectId");
            entity.HasIndex(e => e.InvestorId).HasDatabaseName("IX_ProjectSub_InvestorId");
            entity.Property(e => e.MonthlyAmount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Project).WithMany(p => p.Subscriptions).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Investor).WithMany().HasForeignKey(e => e.InvestorId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ProjectMember>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ProjectMember_ProjectId");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ProjectMember_UserId");
            entity.HasIndex(e => new { e.ProjectId, e.UserId }).HasDatabaseName("IX_ProjectMember_Project_User");

            // "Who is on the client side of this project?" and "who mediates?"
            // are asked on every access resolution — keep both index seeks.
            entity.HasIndex(e => new { e.ProjectId, e.Side }).HasDatabaseName("IX_ProjectMember_Project_Side");
            entity.HasIndex(e => new { e.ProjectId, e.IsMediator }).HasDatabaseName("IX_ProjectMember_Project_Mediator");
            entity.HasOne(e => e.Project).WithMany(p => p.Members).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedBy).WithMany().HasForeignKey(e => e.AssignedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // One reader's arrangement of their own projects. Read once per
        // dashboard load and keyed by the reader, so the composite index is the
        // only one that matters; the unique constraint is what stops a
        // double-tapped drop from writing two positions for one project.
        modelBuilder.Entity<tbl_ProjectPreference>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProjectId })
                  .HasDatabaseName("IX_ProjectPreference_User_Project")
                  .IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsPinned }).HasDatabaseName("IX_ProjectPreference_User_Pinned");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_Flag>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Flag_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_Flag_StageId");
            entity.HasIndex(e => e.ProgressUpdateId).HasDatabaseName("IX_Flag_ProgressUpdateId");
            entity.HasIndex(e => e.ProgressImageId).HasDatabaseName("IX_Flag_ProgressImageId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Flag_Status");
            entity.HasIndex(e => e.AssignedToId).HasDatabaseName("IX_Flag_AssignedToId");
            entity.HasOne(e => e.Project).WithMany(p => p.Flags).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProgressUpdate).WithMany(u => u.Flags).HasForeignKey(e => e.ProgressUpdateId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProgressImage).WithMany(i => i.Flags).HasForeignKey(e => e.ProgressImageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedTo).WithMany().HasForeignKey(e => e.AssignedToId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ResolvedBy).WithMany().HasForeignKey(e => e.ResolvedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => e.CommitmentId).HasDatabaseName("IX_Flag_CommitmentId");
            entity.HasOne(e => e.Commitment).WithMany().HasForeignKey(e => e.CommitmentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.OwnerMember).WithMany().HasForeignKey(e => e.OwnerMemberId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_BudgetLineItem>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_BudgetLineItem_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_BudgetLineItem_StageId");
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_BudgetLineItem_Category");
            entity.Property(e => e.PlannedAmount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_Receipt>(entity =>
        {
            entity.HasIndex(e => e.BudgetLineItemId).HasDatabaseName("IX_Receipt_BudgetLineItemId");
            entity.HasIndex(e => e.PaymentDate).HasDatabaseName("IX_Receipt_PaymentDate");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.BudgetLineItem).WithMany(b => b.Receipts).HasForeignKey(e => e.BudgetLineItemId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_TenantMembership>(entity =>
        {
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_TenantMembership_UserId");
            entity.HasIndex(e => new { e.UserId, e.TenantId })
                  .IsUnique()
                  .HasDatabaseName("UX_TenantMembership_User_Tenant");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Artifact store ────────────────────────────────────────
        modelBuilder.Entity<tbl_Artifact>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Artifact_ProjectId");

            // Law 2 enforced in the schema, not just in code: the same bytes
            // cannot become two artifacts on one project. ArtifactDAL catches
            // the violation and adopts the winner, so a race dedupes too.
            entity.HasIndex(e => new { e.ProjectId, e.Sha256 })
                  .IsUnique()
                  .HasDatabaseName("UX_Artifact_Project_Sha256");

            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UploadedBy).WithMany().HasForeignKey(e => e.UploadedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ArtifactRef>(entity =>
        {
            entity.HasIndex(e => e.ArtifactId).HasDatabaseName("IX_ArtifactRef_ArtifactId");
            entity.HasIndex(e => new { e.TargetType, e.TargetId }).HasDatabaseName("IX_ArtifactRef_Target");

            // The Client Brief reads "everything exposed on this project" —
            // make that one index seek rather than a scan.
            entity.HasIndex(e => new { e.ProjectId, e.Channel }).HasDatabaseName("IX_ArtifactRef_Project_Channel");

            // One artifact points at one target once. Re-attaching is a no-op,
            // which is how a re-send stops producing a duplicate.
            entity.HasIndex(e => new { e.ArtifactId, e.TargetType, e.TargetId })
                  .IsUnique()
                  .HasDatabaseName("UX_ArtifactRef_Artifact_Target");

            entity.HasOne(e => e.Artifact).WithMany(a => a.References).HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ExposedBy).WithMany().HasForeignKey(e => e.ExposedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_Document>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Document_ProjectId");
            entity.HasIndex(e => e.Kind).HasDatabaseName("IX_Document_Kind");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_ArtifactRevision>(entity =>
        {
            entity.HasIndex(e => e.DocumentId).HasDatabaseName("IX_ArtifactRevision_DocumentId");
            entity.HasIndex(e => new { e.DocumentId, e.RevisionNo })
                  .IsUnique()
                  .HasDatabaseName("UX_ArtifactRevision_Document_RevisionNo");

            entity.HasOne(e => e.Document).WithMany(d => d.Revisions).HasForeignKey(e => e.DocumentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.IssuedBy).WithMany().HasForeignKey(e => e.IssuedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── Ingest — the front door ───────────────────────────────
        modelBuilder.Entity<tbl_IngestBatch>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_IngestBatch_ProjectId");
            entity.HasIndex(e => new { e.ProjectId, e.SourceType }).HasDatabaseName("IX_IngestBatch_Project_Source");

            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ImportedBy).WithMany().HasForeignKey(e => e.ImportedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ArchiveArtifact).WithMany().HasForeignKey(e => e.ArchiveArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_IngestedMessage>(entity =>
        {
            entity.HasIndex(e => e.BatchId).HasDatabaseName("IX_IngestedMessage_BatchId");

            // The register reads a project's history in date order and P5 walks
            // it the same way. One composite index serves both.
            entity.HasIndex(e => new { e.ProjectId, e.SentAt }).HasDatabaseName("IX_IngestedMessage_Project_SentAt");

            // Re-import safety enforced in the schema, not only in code: an
            // overlapping export cannot create a second copy of a message even
            // if two imports race. The unique constraint is the guarantee; the
            // pre-check in IngestDAL is the fast path.
            entity.HasIndex(e => new { e.ProjectId, e.DedupeKey })
                  .IsUnique()
                  .HasDatabaseName("UX_IngestedMessage_Project_DedupeKey");

            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Batch).WithMany().HasForeignKey(e => e.BatchId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AuthorMember).WithMany().HasForeignKey(e => e.AuthorMemberId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── The commitment model + money ledger (P4) ──────────────
        modelBuilder.Entity<tbl_Deliverable>(entity =>
        {
            entity.HasIndex(e => new { e.StageId, e.DisplayOrder }).HasDatabaseName("IX_Deliverable_Stage_Order");
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Deliverable_ProjectId");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CompletedBy).WithMany().HasForeignKey(e => e.CompletedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CompletionArtifact).WithMany().HasForeignKey(e => e.CompletionArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasIndex(e => new { e.ProjectId, e.PlannedStart }).HasDatabaseName("IX_Deliverable_Project_PlannedStart");
        });

        modelBuilder.Entity<tbl_DeliverableEvent>(entity =>
        {
            entity.HasIndex(e => e.DeliverableId).HasDatabaseName("IX_DeliverableEvent_DeliverableId");
            entity.HasOne(e => e.Deliverable).WithMany().HasForeignKey(e => e.DeliverableId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.By).WithMany().HasForeignKey(e => e.ById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_PlanWait>(entity =>
        {
            entity.HasIndex(e => e.DeliverableId).HasDatabaseName("IX_PlanWait_DeliverableId");
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_PlanWait_ProjectId");
            entity.HasOne(e => e.Deliverable).WithMany().HasForeignKey(e => e.DeliverableId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Predecessor).WithMany().HasForeignKey(e => e.PredecessorId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_WorkSchedule>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).IsUnique().HasDatabaseName("IX_WorkSchedule_ProjectId");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_Commitment>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Commitment_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_Commitment_StageId");
            entity.HasIndex(e => e.DeliverableId).HasDatabaseName("IX_Commitment_DeliverableId");
            entity.HasIndex(e => new { e.ProjectId, e.AccountableMemberId }).HasDatabaseName("IX_Commitment_Project_Accountable");
            entity.HasIndex(e => e.SupersedesId).HasDatabaseName("IX_Commitment_SupersedesId");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Deliverable).WithMany().HasForeignKey(e => e.DeliverableId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AccountableMember).WithMany().HasForeignKey(e => e.AccountableMemberId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AgreedWithMember).WithMany().HasForeignKey(e => e.AgreedWithMemberId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AgreedBy).WithMany().HasForeignKey(e => e.AgreedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.RecordedBy).WithMany().HasForeignKey(e => e.RecordedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Supersedes).WithMany().HasForeignKey(e => e.SupersedesId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.DependsOnStage).WithMany().HasForeignKey(e => e.DependsOnStageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_CommitmentLink>(entity =>
        {
            entity.HasIndex(e => e.CommitmentId).HasDatabaseName("IX_CommitmentLink_CommitmentId");
            // The backlink direction: what is this photo evidence for?
            entity.HasIndex(e => new { e.TargetType, e.TargetId }).HasDatabaseName("IX_CommitmentLink_Target");
            entity.HasIndex(e => new { e.CommitmentId, e.TargetType, e.TargetId, e.Relation })
                  .IsUnique()
                  .HasDatabaseName("UX_CommitmentLink_Commitment_Target_Relation");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Commitment).WithMany().HasForeignKey(e => e.CommitmentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_Variation>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Variation_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_Variation_StageId");
            entity.Property(e => e.CostDelta).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Commitment).WithMany().HasForeignKey(e => e.CommitmentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.RaisedBy).WithMany().HasForeignKey(e => e.RaisedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ApprovedBy).WithMany().HasForeignKey(e => e.ApprovedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_StageClaim>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_StageClaim_ProjectId");
            entity.HasIndex(e => e.StageId).HasDatabaseName("IX_StageClaim_StageId");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ClearedAmount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ClaimedBy).WithMany().HasForeignKey(e => e.ClaimedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ClearedBy).WithMany().HasForeignKey(e => e.ClearedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── Extraction (P5) ───────────────────────────────────────
        modelBuilder.Entity<tbl_ArtifactText>(entity =>
        {
            entity.HasIndex(e => e.ArtifactId).IsUnique().HasDatabaseName("UX_ArtifactText_ArtifactId");
            entity.HasIndex(e => new { e.ProjectId, e.Status }).HasDatabaseName("IX_ArtifactText_Project_Status");
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_ExtractionRun>(entity =>
        {
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_ExtractionRun_ProjectId");
        });

        modelBuilder.Entity<tbl_ExtractionProposal>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Status }).HasDatabaseName("IX_ExtractionProposal_Project_Status");
            entity.HasIndex(e => e.IngestedMessageId).HasDatabaseName("IX_ExtractionProposal_MessageId");

            // A re-run never re-proposes what a person already decided.
            entity.HasIndex(e => new { e.ProjectId, e.Fingerprint })
                  .IsUnique()
                  .HasDatabaseName("UX_ExtractionProposal_Project_Fingerprint");

            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.IngestedMessage).WithMany().HasForeignKey(e => e.IngestedMessageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.DecidedBy).WithMany().HasForeignKey(e => e.DecidedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ProgressReading>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.ObservedAt }).HasDatabaseName("IX_ProgressReading_Project_ObservedAt");
            entity.HasIndex(e => new { e.StageId, e.ObservedAt }).HasDatabaseName("IX_ProgressReading_Stage_ObservedAt");
            entity.Property(e => e.Percent).HasColumnType("decimal(5,2)");
            entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_MediaBinding>(entity =>
        {
            // One file per line: a second re-join of the same folder binds nothing new.
            entity.HasIndex(e => e.IngestedMessageId).IsUnique().HasDatabaseName("UX_MediaBinding_MessageId");
            entity.HasIndex(e => new { e.ProjectId, e.ArtifactId }).HasDatabaseName("IX_MediaBinding_Project_Artifact");
            entity.HasOne(e => e.IngestedMessage).WithMany().HasForeignKey(e => e.IngestedMessageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_WorksReport>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.AsAt }).HasDatabaseName("IX_WorksReport_Project_AsAt");
            entity.HasIndex(e => new { e.ProjectId, e.TriggerKey }).HasDatabaseName("IX_WorksReport_Project_Trigger");
            entity.HasOne(e => e.IssuedBy).WithMany().HasForeignKey(e => e.IssuedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ArtifactPoster>(entity =>
        {
            // One poster per video, however many times the video is referenced.
            entity.HasIndex(e => e.ArtifactId).IsUnique().HasDatabaseName("UX_ArtifactPoster_ArtifactId");
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.PosterArtifact).WithMany().HasForeignKey(e => e.PosterArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── Markup and parked ideas (P8) ──────────────────────────
        modelBuilder.Entity<tbl_Annotation>(entity =>
        {
            entity.HasIndex(e => new { e.ArtifactId, e.LayerId, e.Version }).IsUnique().HasDatabaseName("UX_Annotation_Artifact_Layer_Version");
            entity.HasIndex(e => e.CommitmentId).HasDatabaseName("IX_Annotation_CommitmentId");
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Annotation_ProjectId");
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Author).WithMany().HasForeignKey(e => e.AuthorId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Commitment).WithMany().HasForeignKey(e => e.CommitmentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_CommitmentEstimate>(entity =>
        {
            entity.HasIndex(e => e.CommitmentId).HasDatabaseName("IX_CommitmentEstimate_CommitmentId");
            entity.Property(e => e.Amount).HasColumnType("decimal(18,4)");
            entity.HasOne(e => e.Commitment).WithMany().HasForeignKey(e => e.CommitmentId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.RecordedBy).WithMany().HasForeignKey(e => e.RecordedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── The contractor tier (P9) ──────────────────────────────
        modelBuilder.Entity<tbl_ProgressUpdate>(entity =>
        {
            // The offline queue retries until it hears back; the second arrival is the first one.
            entity.HasIndex(e => new { e.ProjectId, e.ClientCaptureId }).IsUnique()
                .HasFilter("\"ClientCaptureId\" IS NOT NULL")
                .HasDatabaseName("UX_ProgressUpdate_Project_ClientCapture");
            entity.HasIndex(e => e.DeliverableId).HasDatabaseName("IX_ProgressUpdate_DeliverableId");
            entity.HasOne(e => e.Deliverable).WithMany().HasForeignKey(e => e.DeliverableId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.VoiceArtifact).WithMany().HasForeignKey(e => e.VoiceArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_BriefPublication>(entity =>
        {
            entity.HasIndex(e => new { e.ProjectId, e.Day }).IsUnique().HasDatabaseName("UX_BriefPublication_Project_Day");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.PublishedBy).WithMany().HasForeignKey(e => e.PublishedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_ClaimEvidence>(entity =>
        {
            entity.HasIndex(e => e.ClaimId).HasDatabaseName("IX_ClaimEvidence_ClaimId");
            entity.HasOne(e => e.Claim).WithMany().HasForeignKey(e => e.ClaimId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProgressImage).WithMany().HasForeignKey(e => e.ProgressImageId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Artifact).WithMany().HasForeignKey(e => e.ArtifactId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Deliverable).WithMany().HasForeignKey(e => e.DeliverableId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.ProgressReading).WithMany().HasForeignKey(e => e.ProgressReadingId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_PushSubscription>(entity =>
        {
            entity.HasIndex(e => e.EndpointHash).IsUnique().HasDatabaseName("UX_PushSubscription_EndpointHash");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_PushSubscription_UserId");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<tbl_PushDelivery>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.QueuedAt }).HasDatabaseName("IX_PushDelivery_User_QueuedAt");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_PushDelivery_Status");
            entity.HasOne(e => e.Subscription).WithMany().HasForeignKey(e => e.SubscriptionId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
        });

        // ─── Platform tables ───────────────────────────────────────
        modelBuilder.Entity<tbl_Configuration>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("tbl_Configuration");
            entity.Property(e => e.ConfigId).ValueGeneratedNever().HasColumnName("SettingID");
        });

        modelBuilder.Entity<tbl_RoleValue>(entity =>
        {
            entity.ToTable("tbl_RoleValues");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<tbl_Tenant>(entity =>
        {
            entity.Property(e => e.TenantId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.TenantId).HasDefaultValueSql("gen_random_uuid()::text");
        });

        modelBuilder.Entity<tbl_RefreshToken>()
            .HasIndex(e => e.Token)
            .HasDatabaseName("IX_tbl_RefreshToken_token");

        modelBuilder.Entity<tbl_RefreshToken>()
            .HasIndex(e => e.DeviceFingerprint)
            .HasDatabaseName("IX_tbl_RefreshToken_deviceFingerprint");

        modelBuilder.Entity<tbl_SubscriptionRequest>(entity =>
        {
            entity.ToTable("tbl_SubscriptionRequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuotedAmount).HasColumnType("decimal(18, 2)");
            entity.HasMany(e => e.Seats)
                  .WithOne(s => s.Request)
                  .HasForeignKey(s => s.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<tbl_SubscriptionSeat>(entity =>
        {
            entity.ToTable("tbl_SubscriptionSeats");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RequestId, e.Email }).IsUnique();
        });

        // Two-admin approval before a general user becomes an employee.
        modelBuilder.Entity<tbl_EmployeeApproval>()
            .HasOne(e => e.TargetUser)
            .WithMany()
            .HasForeignKey(e => e.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<tbl_EmployeeApproval>()
            .HasIndex(e => new { e.TargetUserId, e.ApproverUserId })
            .HasDatabaseName("IX_tbl_EmployeeApproval_TargetUser_Approver");

        modelBuilder.Entity<tbl_EmployeeApproval>()
            .HasIndex(e => e.TargetUserId)
            .HasDatabaseName("IX_tbl_EmployeeApproval_TargetUserId");

        // ─── Conventions applied to every BaseEntity ───────────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) || typeof(IBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.IsDeleted));

                // Configure TenantId as nullable (optional) for all entities except tbl_Tenant itself
                if (entityType.ClrType != typeof(tbl_Tenant))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.TenantId))
                        .IsRequired(false);
                }

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.DateTimeCreated));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.DateTimeModified));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.LastModifiedBy));
            }
        }

        ConfigurePostgres(modelBuilder);

        modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
        modelBuilder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <summary>
    /// Every DateTime is <c>timestamptz</c> and reaches Postgres labelled UTC with
    /// its numbers untouched; it comes back unlabelled, as it did from datetime2.
    /// Instants are written as UtcNow; the project's wall-clock values (a message's
    /// sent time, a photo's shot time, a report's as-at day) are carried unshifted.
    /// One column type means no comparison in SQL ever depends on the session
    /// TimeZone (CLAUDE.md §5.1.1).
    /// </summary>
    private static readonly ValueConverter<DateTime, DateTime> PostgresDateTime = new(
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified));

    private static void ConfigurePostgres(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(PostgresDateTime);

        // Search (SearchDAL) asks ILIKE and word_similarity of these columns; a
        // trigram GIN index serves both, so an OCR misread still finds its receipt.
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.Entity<tbl_IngestedMessage>().HasIndex(e => e.Body)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_IngestedMessage_Body_trgm");
        modelBuilder.Entity<tbl_ArtifactText>().HasIndex(e => e.Text)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_ArtifactText_Text_trgm");
        modelBuilder.Entity<tbl_Commitment>().HasIndex(e => e.Title)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_Commitment_Title_trgm");
        modelBuilder.Entity<tbl_Commitment>().HasIndex(e => e.Body)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_Commitment_Body_trgm");
        modelBuilder.Entity<tbl_ProgressUpdate>().HasIndex(e => e.Description)
            .HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_ProgressUpdate_Description_trgm");
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// PostgreSQL <c>text</c> cannot hold U+0000, and it arrives in pasted PDF text,
    /// OCR output and forwarded mail; one would fail the whole save (CLAUDE.md §5.1.1).
    /// </summary>
    private static void StripNulCharacters(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified)) return;
        foreach (var p in entry.Properties)
            if (p.Metadata.ClrType == typeof(string) && p.CurrentValue is string s && s.Contains('\0'))
                p.CurrentValue = s.Replace("\0", "");
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries();
        foreach (var entity in entries)
        {
            StripNulCharacters(entity);

            if (entity.State == EntityState.Added)
            {
                // Set DateTimeCreated if exists
                var dateTimeCreatedProp = entity.Metadata.FindProperty("DateTimeCreated");
                if (dateTimeCreatedProp != null && IsDateTimeType(dateTimeCreatedProp))
                {
                    entity.Property("DateTimeCreated").CurrentValue = DateTime.UtcNow;
                }

                // Set DateTimeModified if exists
                var dateTimeModifiedProp = entity.Metadata.FindProperty("DateTimeModified");
                if (dateTimeModifiedProp != null && IsDateTimeType(dateTimeModifiedProp))
                {
                    entity.Property("DateTimeModified").CurrentValue = DateTime.UtcNow;
                }

                // Set LastModifiedBy if exists and is string
                var lastModifiedByProp = entity.Metadata.FindProperty("LastModifiedBy");
                if (lastModifiedByProp != null && lastModifiedByProp.ClrType == typeof(string))
                {
                    entity.Property("LastModifiedBy").CurrentValue = _tenantProvider.GetUserId();
                }

                // Child rows read OwnerTenantId back through ResolveOwningTenantId, so
                // a null here would send them to whichever tenant wrote them. ProjectDAL
                // sets it for sub-projects; this is the backstop for every other path.
                if (entity.Entity is tbl_Project newProject
                    && string.IsNullOrEmpty(newProject.OwnerTenantId))
                {
                    newProject.OwnerTenantId = _tenantProvider.GetTenantId();
                }

                // Stamped with the PROJECT OWNER, not the writer. Peter owns the project
                // (D1) and contractors are guests in it — stamping from the writer would
                // put a guest comment in the guest tenant, where the filter hides it.
                var tenantIdProp = entity.Metadata.FindProperty("TenantId");
                if (tenantIdProp != null && tenantIdProp.ClrType == typeof(string))
                {
                    var tenantIdEntry = entity.Property("TenantId");
                    if (tenantIdEntry.CurrentValue == null || (string?)tenantIdEntry.CurrentValue == "")
                    {
                        tenantIdEntry.CurrentValue =
                            ResolveOwningTenantId(entity) ?? _tenantProvider.GetTenantId();
                    }
                }

                // Set Id if exists, is string, and is empty
                var idProp = entity.Metadata.FindProperty("Id");
                if (idProp != null && idProp.ClrType == typeof(string))
                {
                    var idEntry = entity.Property("Id");
                    var currentId = (string)idEntry.CurrentValue;
                    if (string.IsNullOrEmpty(currentId))
                    {
                        idEntry.CurrentValue = Guid.NewGuid().ToString();
                    }
                }
            }
            else if (entity.State == EntityState.Modified)
            {
                // Set DateTimeModified if exists
                var dateTimeModifiedProp = entity.Metadata.FindProperty("DateTimeModified");
                if (dateTimeModifiedProp != null && IsDateTimeType(dateTimeModifiedProp))
                {
                    entity.Property("DateTimeModified").CurrentValue = DateTime.UtcNow;
                }

                // Prevent DateTimeCreated from being modified
                var dateTimeCreatedProp = entity.Metadata.FindProperty("DateTimeCreated");
                if (dateTimeCreatedProp != null)
                {
                    entity.Property("DateTimeCreated").IsModified = false;
                }

                // Set LastModifiedBy if exists and is string
                var lastModifiedByProp = entity.Metadata.FindProperty("LastModifiedBy");
                if (lastModifiedByProp != null && lastModifiedByProp.ClrType == typeof(string))
                {
                    entity.Property("LastModifiedBy").CurrentValue = _tenantProvider.GetUserId();
                }
                // Stamped with the PROJECT OWNER, not the writer. Peter owns the project
                // (D1) and contractors are guests in it — stamping from the writer would
                // put a guest comment in the guest tenant, where the filter hides it.
                var tenantIdProp = entity.Metadata.FindProperty("TenantId");
                if (tenantIdProp != null && tenantIdProp.ClrType == typeof(string))
                {
                    var tenantIdEntry = entity.Property("TenantId");
                    if (tenantIdEntry.CurrentValue == null || (string?)tenantIdEntry.CurrentValue == "")
                    {
                        tenantIdEntry.CurrentValue =
                            ResolveOwningTenantId(entity) ?? _tenantProvider.GetTenantId();
                    }
                }
            }
        }
    }

    /// <summary>
    /// The tenant that owns the row — the <em>project</em> owner, not the caller.
    /// Null for platform rows with no project, and the caller falls back to their own.
    /// </summary>
    private string? ResolveOwningTenantId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity)
    {
        // A project carries its own owner.
        if (entity.Entity is tbl_Project project)
            return project.OwnerTenantId;

        var projectIdProp = entity.Metadata.FindProperty("ProjectId");
        if (projectIdProp is null || projectIdProp.ClrType != typeof(string))
            return null;

        var projectId = entity.Property("ProjectId").CurrentValue as string;
        if (string.IsNullOrEmpty(projectId))
            return null;

        // Usually already tracked: the DAL just loaded it to authorize the write.
        var tracked = ChangeTracker.Entries<tbl_Project>()
            .FirstOrDefault(e => e.Entity.Id == projectId)?.Entity;
        if (tracked is not null)
            return tracked.OwnerTenantId;

        // IgnoreQueryFilters: a guest cannot see the owner project through the
        // tenant filter, which is the situation this method exists for.
        return tbl_Projects_RS
            .IgnoreQueryFilters()
            .Where(p => p.Id == projectId)
            .Select(p => p.OwnerTenantId)
            .FirstOrDefault();
    }

    // Helper to check if property is DateTime or DateTime? (using IProperty)
    private bool IsDateTimeType(Microsoft.EntityFrameworkCore.Metadata.IProperty property)
    {
        var type = property.ClrType;
        return type == typeof(DateTime) || type == typeof(DateTime?);
    }
}
