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

public static class Converters
{
    public static readonly ValueConverter<decimal, string> DecimalToStringConverter =
        new ValueConverter<decimal, string>(
            v => v.ToString(CultureInfo.InvariantCulture),
            v => decimal.Parse(v, CultureInfo.InvariantCulture));
}

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

    // ─── Projects + Site Log (ASSETLEN core) ───────────────────
    public virtual DbSet<tbl_Project> tbl_Projects_RS { get; set; }
    public virtual DbSet<tbl_Stage> tbl_Stages { get; set; }
    public virtual DbSet<tbl_FundingEntry> tbl_FundingEntries { get; set; }
    public virtual DbSet<tbl_ProgressUpdate> tbl_ProgressUpdates { get; set; }
    public virtual DbSet<tbl_ProgressImage> tbl_ProgressImages { get; set; }
    public virtual DbSet<tbl_ProgressComment> tbl_ProgressComments { get; set; }
    public virtual DbSet<tbl_ProjectSubscription> tbl_ProjectSubscriptions { get; set; }
    public virtual DbSet<tbl_Flag> tbl_Flags { get; set; }
    public virtual DbSet<tbl_ProjectMember> tbl_ProjectMembers { get; set; }
    public virtual DbSet<tbl_BudgetLineItem> tbl_BudgetLineItems { get; set; }
    public virtual DbSet<tbl_Receipt> tbl_Receipts { get; set; }

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
        TenantScoped<tbl_BudgetLineItem>(modelBuilder);
        TenantScoped<tbl_Receipt>(modelBuilder);

        // Channel-based (Client/Crew) visibility is enforced at the service
        // layer — DbContext-level filtering would need ITenantProvider to
        // expose role information. Added in P5 when the Client Brief ships.

        // ─── Projects + Site Log relationships ─────────────────────
        modelBuilder.Entity<tbl_Project>(entity =>
        {
            entity.ToTable("tbl_Projects_RS");
            entity.HasIndex(e => e.InvestorId).HasDatabaseName("IX_Project_InvestorId");
            entity.HasIndex(e => e.ProjectManagerId).HasDatabaseName("IX_Project_ProjectManagerId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Project_Status");
            entity.HasIndex(e => e.ParentProjectId).HasDatabaseName("IX_Project_ParentProjectId");
            entity.Property(e => e.TotalBudget).HasColumnType("decimal(18,4)");
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
            entity.HasOne(e => e.Project).WithMany(p => p.Members).HasForeignKey(e => e.ProjectId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedBy).WithMany().HasForeignKey(e => e.AssignedById).IsRequired(false).OnDelete(DeleteBehavior.NoAction);
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
            if (Database.IsSqlServer())
            {
                entity.Property(e => e.TenantId).HasDefaultValueSql("NEWID()");
            }
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

        if (Database.IsSqlServer())
        {
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
        }

        modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
        modelBuilder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        // SQLite-specific configurations
        if (Database.IsSqlite())
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetColumnType("TEXT");
                        property.SetValueConverter(Converters.DecimalToStringConverter);
                    }
                    else if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("TEXT");
                    }
                }
            }
        }

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

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
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries();
        foreach (var entity in entries)
        {
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

                // Set TenantId if exists, is string, and is null
                var tenantIdProp = entity.Metadata.FindProperty("TenantId");
                if (tenantIdProp != null && tenantIdProp.ClrType == typeof(string))
                {
                    var tenantIdEntry = entity.Property("TenantId");
                    if (tenantIdEntry.CurrentValue == null || tenantIdEntry.CurrentValue == "")
                    {
                        tenantIdEntry.CurrentValue = _tenantProvider.GetTenantId();
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
                // Set TenantId if exists, is string, and is null
                var tenantIdProp = entity.Metadata.FindProperty("TenantId");
                if (tenantIdProp != null && tenantIdProp.ClrType == typeof(string))
                {
                    var tenantIdEntry = entity.Property("TenantId");
                    if (tenantIdEntry.CurrentValue == null || tenantIdEntry.CurrentValue == "")
                    {
                        tenantIdEntry.CurrentValue = _tenantProvider.GetTenantId();
                    }
                }
            }
        }
    }

    // Helper to check if property is DateTime or DateTime? (using IProperty)
    private bool IsDateTimeType(Microsoft.EntityFrameworkCore.Metadata.IProperty property)
    {
        var type = property.ClrType;
        return type == typeof(DateTime) || type == typeof(DateTime?);
    }
}
