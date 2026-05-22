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

    // DbSet properties remain unchanged
    public virtual DbSet<tbl_CashItem> tbl_CashItems { get; set; }
    public virtual DbSet<tbl_Tenant> tbl_Tenants { get; set; }
    public virtual DbSet<tbl_Category> tbl_Categories { get; set; }
    public virtual DbSet<tbl_Configuration> tbl_Configurations { get; set; }
    public virtual DbSet<tbl_Customer> tbl_Customers { get; set; }
    public virtual DbSet<tbl_CustomerPricing> tbl_CustomerPricings { get; set; }
    public virtual DbSet<tbl_Discount> tbl_Discounts { get; set; }
    public virtual DbSet<tbl_Expense> tbl_Expenses { get; set; }
    public virtual DbSet<tbl_ExpenseType> tbl_ExpenseTypes { get; set; }
    public virtual DbSet<tbl_Location> tbl_Locations { get; set; }
    public virtual DbSet<tbl_Log> tbl_Logs { get; set; }
    public virtual DbSet<tbl_OrderProcess> tbl_OrderProcesses { get; set; }
    public virtual DbSet<tbl_OrderStatus> tbl_OrderStatuses { get; set; }
    public virtual DbSet<tbl_Payment> tbl_Payments { get; set; }
    public virtual DbSet<tbl_PaymentAccount> tbl_PaymentAccounts { get; set; }
    public virtual DbSet<tbl_PaymentMode> tbl_PaymentModes { get; set; }
    public virtual DbSet<tbl_Bank> tbl_Banks { get; set; }
    public virtual DbSet<tbl_Product> tbl_Products { get; set; }
    public virtual DbSet<tbl_ProductReceiving> tbl_ProductReceivings { get; set; }
    public virtual DbSet<tbl_ProductRelationship> tbl_ProductRelationships { get; set; }
    public virtual DbSet<tbl_RoleValue> tbl_RoleValues { get; set; }
    public virtual DbSet<tbl_Segment> tbl_Segments { get; set; }
    public virtual DbSet<tbl_Shift> tbl_Shifts { get; set; }
    public virtual DbSet<tbl_Size> tbl_Sizes { get; set; }
    public virtual DbSet<tbl_SlipLayout> tbl_SlipLayouts { get; set; }
    public virtual DbSet<tbl_Supplier> tbl_Suppliers { get; set; }
    public virtual DbSet<tbl_SupplierPayment> tbl_SupplierPayments { get; set; }
    public virtual DbSet<tbl_Tax> tbl_Taxes { get; set; }
    public virtual DbSet<tbl_Transaction> tbl_Transactions { get; set; }
    public virtual DbSet<tbl_TransactionDetail> tbl_TransactionDetails { get; set; }
    public virtual DbSet<tbl_UniqueField> tbl_UniqueFields { get; set; }
    public virtual DbSet<tbl_ShiftClosureSummary> tbl_ShiftClosureSummaries { get; set; }
    public virtual DbSet<tbl_SyncLog> tbl_SyncLogs { get; set; }
    public virtual DbSet<tbl_PrinterPreferances> tbl_PrinterPreferances { get; set; }
    public virtual DbSet<tbl_ProductDetail> tbl_ProductDetails { get; set; }
    public virtual DbSet<tbl_ProductDetailFeedback> tbl_ProductDetailFeedbacks { get; set; }
    public virtual DbSet<tbl_ProductDetailFeedbackReply> tbl_ProductDetailFeedbackReplies { get; set; }
    public virtual DbSet<tbl_FeedbackApproval> tbl_FeedbackApprovals { get; set; }
    public virtual DbSet<tbl_EmployeeApproval> tbl_EmployeeApprovals { get; set; }
    public DbSet<tbl_RefreshToken> RefreshTokens { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<tbl_UserFavorite> tbl_UserFavorites { get; set; }
    public DbSet<tbl_UserDocument> tbl_UserDocuments { get; set; }
    public DbSet<tbl_SubscriptionRequest> tbl_SubscriptionRequests { get; set; }
    public DbSet<tbl_SubscriptionSeat> tbl_SubscriptionSeats { get; set; }

    // ─── Projects + Site Journal (ASSETLEN core) ───────────────
    public virtual DbSet<tbl_Project> tbl_Projects_RS { get; set; }
    public virtual DbSet<tbl_Stage> tbl_Stages { get; set; }
    public virtual DbSet<tbl_FundingEntry> tbl_FundingEntries { get; set; }
    public virtual DbSet<tbl_ProgressUpdate> tbl_ProgressUpdates { get; set; }
    public virtual DbSet<tbl_ProgressImage> tbl_ProgressImages { get; set; }
    public virtual DbSet<tbl_ProgressComment> tbl_ProgressComments { get; set; }
    public virtual DbSet<tbl_ProjectSubscription> tbl_ProjectSubscriptions { get; set; }
    public virtual DbSet<tbl_Flag> tbl_Flags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register Identity defaults first — .NET 10 added IdentityPasskeyData
        // and similar passkey/WebAuthn types that need their key config from
        // the base. Custom configurations below may override individual bits.
        base.OnModelCreating(modelBuilder);

        // ─── Multi-tenant + Access query filters ──────────────────────
        // Pattern: (SuperAdmin OR same tenant OR null tenant OR Public access)
        //          AND (access is null or not Protected)
        //          AND (not soft-deleted)
        modelBuilder.Entity<tbl_CashItem>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Category>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Configuration>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Customer>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_CustomerPricing>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Discount>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Expense>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ExpenseType>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Location>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Log>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_OrderProcess>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_OrderStatus>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Payment>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_PaymentAccount>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        //modelBuilder.Entity<tbl_PaymentMode>().HasQueryFilter(x => x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Bank>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Product>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProductReceiving>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProductRelationship>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_RoleValue>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Segment>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Shift>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Size>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_SlipLayout>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Supplier>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_SupplierPayment>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Tax>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Transaction>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_TransactionDetail>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_UniqueField>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<AppUser>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ShiftClosureSummary>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_SyncLog>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_PrinterPreferances>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProductDetail>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProductDetailFeedback>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProductDetailFeedbackReply>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_RefreshToken>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_FeedbackApproval>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_UserFavorite>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_UserDocument>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));

        // ─── Remote Site query filters ─────────────────────────────
        modelBuilder.Entity<tbl_Project>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Stage>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_FundingEntry>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProgressUpdate>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProgressImage>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProgressComment>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_ProjectSubscription>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));
        modelBuilder.Entity<tbl_Flag>().HasQueryFilter(x =>
            (_isSuperAdmin || x.TenantId == _tenantId || x.TenantId == null || x.Access == Access.Public)
            && (x.Access == null || x.Access != Access.Protected)
            && (x.IsDeleted == false || x.IsDeleted == null));

        // Channel-based (Client/Crew) visibility is enforced at the service
        // layer — DbContext-level filtering would need ITenantProvider to
        // expose role information. Added in Phase 2 when Streams ship.

        // ─── Projects + Site Journal relationships ─────────────────
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

        // EmployeeApproval relationship
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

        // Feedback relationships
        modelBuilder.Entity<tbl_ProductDetailFeedbackReply>()
            .HasOne(r => r.Feedback)
            .WithMany(f => f.Replies)
            .HasForeignKey(r => r.FeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<tbl_ProductDetailFeedbackReply>()
            .HasOne(r => r.ParentReply)
            .WithMany(r => r.ChildReplies)
            .HasForeignKey(r => r.ParentReplyId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<tbl_TransactionDetail>()
            .HasOne(td => td.Tax)
            .WithMany()
            .HasForeignKey(td => td.TaxId);
        modelBuilder.Entity<tbl_TransactionDetail>()
            .HasOne(td => td.Product)
            .WithMany()
            .HasForeignKey(td => td.ProductId);
        modelBuilder.Entity<tbl_TransactionDetail>()
            .HasOne(td => td.Discount)
            .WithMany()
            .HasForeignKey(td => td.DiscountId);
        modelBuilder.Entity<tbl_Transaction>()
            .HasOne(td => td.Customer)
            .WithMany(t => t.Transactions)
            .HasForeignKey(td => td.CustomerId)
            .IsRequired(false);
        modelBuilder.Entity<tbl_Transaction>()
            .HasOne(td => td.Seller)
            .WithMany()
            .HasForeignKey(td => td.SoldBy);
        modelBuilder.Entity<tbl_Transaction>()
            .HasOne(td => td.SaleAgent)
            .WithMany()
            .HasForeignKey(td => td.SaleAgentId);

        modelBuilder.Entity<tbl_Payment>()
            .HasOne(td => td.PaymentMode)
            .WithMany()
            .HasForeignKey(td => td.PaymentModeId);

        modelBuilder.Entity<tbl_Payment>()
            .HasOne(td => td.Supplier)
            .WithMany()
            .HasForeignKey(td => td.SupplierId);

        modelBuilder.Entity<tbl_Payment>()
            .HasOne(td => td.SupplierPayment)
            .WithMany()
            .HasForeignKey(td => td.SupplierPaymentId);

        modelBuilder.Entity<tbl_Payment>()
            .HasOne(td => td.Employee)
            .WithMany()
            .HasForeignKey(td => td.EmployeeId);
        modelBuilder.Entity<tbl_Payment>()
            .HasOne<tbl_Bank>()
            .WithMany()
            .HasForeignKey(td => td.BankId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<tbl_ProductDetail>()
            .HasOne<tbl_Product>()
            .WithMany()
            .HasForeignKey(td => td.ProductId);

        modelBuilder.Entity<tbl_Transaction>(entity =>
        {
            entity.HasIndex(e => e.TransactionDate)
                  .HasDatabaseName("IX_tbl_transaction_transactionDate");
            entity.HasIndex(e => e.TransactionStatus)
                  .HasDatabaseName("IX_tbl_transaction_transactionStatus");
        });

        modelBuilder.Entity<tbl_TransactionDetail>(entity =>
        {
            entity.HasIndex(e => e.TransactionId)
                  .HasDatabaseName("IX_tbl_transactionDetail_transactionID");
            entity.HasIndex(e => e.ProductId)
                  .HasDatabaseName("IX_tbl_transactionDetail_productID");
        });

        modelBuilder.Entity<tbl_Transaction>()
            .HasIndex(e => e.CustomerId)
            .HasDatabaseName("IX_tbl_transaction_customerId");

        modelBuilder.Entity<tbl_Transaction>()
            .HasIndex(e => new { e.TransactionDate, e.CustomerId })
            .HasDatabaseName("IX_tbl_transaction_transactionDate_customerId");

        modelBuilder.Entity<tbl_Transaction>()
            .HasIndex(e => e.SaleAgentId)
            .HasDatabaseName("IX_tbl_transaction_supplierId");

        modelBuilder.Entity<tbl_Transaction>()
            .HasIndex(e => new { e.TransactionDate, e.SaleAgentId })
            .HasDatabaseName("IX_tbl_transaction_transactionDate_SaleAgentId");

        modelBuilder.Entity<tbl_SupplierPayment>()
            .HasIndex(e => e.SupplierId)
            .HasDatabaseName("IX_tbl_SupplierPayment_supplierID");

        modelBuilder.Entity<tbl_SupplierPayment>()
            .HasIndex(e => e.DateTimePayed)
            .HasDatabaseName("IX_tbl_SupplierPayment_dateTimePayed");

        modelBuilder.Entity<tbl_RefreshToken>()
      .HasIndex(e => e.Token)
      .HasDatabaseName("IX_tbl_RefreshToken_token");

        modelBuilder.Entity<tbl_RefreshToken>()
     .HasIndex(e => e.DeviceFingerprint)
     .HasDatabaseName("IX_tbl_RefreshToken_deviceFingerprint");

        modelBuilder.Entity<tbl_SupplierPayment>()
            .HasIndex(e => new { e.DateTimePayed, e.SupplierId })
            .HasDatabaseName("IX_tbl_SupplierPayment_dateTimePayed_supplierID");

        modelBuilder.Entity<tbl_Payment>()
            .HasIndex(e => e.SaleId)
            .HasDatabaseName("IX_tbl_Payments_saleID");

        modelBuilder.Entity<tbl_Payment>()
            .HasIndex(e => e.PaymentModeId)
            .HasDatabaseName("IX_tbl_Payments_PaymentModeID");

        modelBuilder.Entity<tbl_Payment>()
            .HasIndex(e => new { e.SaleId, e.PaymentModeId })
            .HasDatabaseName("IX_tbl_Payments_saleID_PaymentModeID");

        modelBuilder.Entity<tbl_Payment>()
            .HasIndex(e => new { e.SaleId, e.PaymentModeId })
            .HasDatabaseName("IX_tbl_Payments_purchaseID_PaymentModeID");

        modelBuilder.Entity<tbl_PaymentMode>()
            .HasIndex(e => e.Id)
            .HasDatabaseName("IX_tbl_paymentMode_PaymentModeID");

        modelBuilder.Entity<tbl_Bank>(entity =>
        {
            entity.ToTable("tbl_Banks");
            entity.Property(e => e.BankName).HasMaxLength(200).IsUnicode(false).IsRequired();
            entity.Property(e => e.SwiftCode).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(500).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(1000).IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        modelBuilder.Entity<tbl_Product>(entity =>
        {
            entity.HasIndex(e => e.CategoryId)
                  .HasDatabaseName("IX_tbl_products_categoryId");
        });

        modelBuilder.Entity<tbl_Product>(entity =>
        {
            entity.HasIndex(e => e.SegmentId)
                  .HasDatabaseName("IX_tbl_products_segmentId");
        });

        modelBuilder.Entity<tbl_Product>(entity =>
        {
            entity.HasIndex(e => new { e.CategoryId, e.SegmentId })
                  .HasDatabaseName("IX_tbl_products_categoryId_segmentId");
        });

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

                //modelBuilder.Entity(entityType.ClrType)
                //    .HasIndex(nameof(BaseEntity.TenantId));

                //if (entityType.ClrType != typeof(tbl_Tenant) && typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                //{
                //    modelBuilder.Entity(entityType.ClrType)
                //        .HasOne(typeof(tbl_Tenant))
                //        .WithMany()
                //        .HasForeignKey(nameof(BaseEntity.TenantId))
                //        .IsRequired(false)
                //        .OnDelete(DeleteBehavior.Restrict);
                //}

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.DateTimeCreated));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.DateTimeModified));

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(BaseEntity.LastModifiedBy));

            }
        }
        modelBuilder.Entity<tbl_Transaction>(entity =>
        {
            entity.HasIndex(e => new { e.TransactionDate, e.TransactionStatus })
                  .HasDatabaseName("IX_tbl_transaction_transactionDate_transactionStatus");
        });

        if (Database.IsSqlServer())
        {
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
        }

        modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
        modelBuilder.Entity<IdentityUserRole<string>>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<IdentityUserToken<string>>().HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        modelBuilder.Entity<tbl_CashItem>(entity =>
        {
            entity.ToTable("tbl_CashItems");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            //entity.HasIndex(e => e.Amount).IsUnique();
        });

        modelBuilder.Entity<tbl_Category>(entity =>
        {
            entity.ToTable("tbl_category");
            entity.Property(e => e.Category).HasMaxLength(100).IsUnicode(false).HasColumnName("category");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Description).HasMaxLength(100).IsUnicode(false).HasColumnName("description");
            entity.Property(e => e.HideInPos).HasColumnName("hideInPOS");
        });

        modelBuilder.Entity<tbl_Configuration>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.ToTable("tbl_Configuration");
            entity.Property(e => e.ConfigId).ValueGeneratedNever().HasColumnName("SettingID");
        });

        modelBuilder.Entity<tbl_Customer>(entity =>
        {
            entity.ToTable("tbl_Customers");
            entity.Property(e => e.AccountNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.CardNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Company).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Contact).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)").HasColumnName("creditLimit");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.VatNumber).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<tbl_CustomerPricing>(entity =>
        {
            entity.ToTable("tbl_customerPricing");
            entity.Property(e => e.CostExc).HasColumnType("decimal(18, 4)").HasColumnName("costExc");
            entity.Property(e => e.CostInc).HasColumnType("decimal(18, 4)").HasColumnName("costInc");
            entity.Property(e => e.CustomerId).HasColumnName("customerID");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.PriceExc).HasColumnType("decimal(18, 4)").HasColumnName("priceExc");
            entity.Property(e => e.PriceGroupId).HasColumnName("priceGroupID");
            entity.Property(e => e.PriceInc).HasColumnType("decimal(18, 4)").HasColumnName("priceInc");
            entity.Property(e => e.ProductId).HasColumnName("productID");
            entity.Property(e => e.SortOrder).HasColumnName("sortOrder");
            entity.Property(e => e.TaxId).HasColumnName("taxID");
        });

        modelBuilder.Entity<tbl_Discount>(entity =>
        {
            entity.ToTable("tbl_discounts");
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 4)").HasColumnName("discountValue");
        });

        modelBuilder.Entity<tbl_Tenant>(entity =>
        {
            entity.Property(e => e.TenantId).HasMaxLength(36).IsRequired();
            if (Database.IsSqlServer())
            {
                entity.Property(e => e.TenantId).HasDefaultValueSql("NEWID()");
            }
        });

        modelBuilder.Entity<tbl_Expense>(entity =>
        {
            entity.ToTable("tbl_Expense");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Comment).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DateTimePayed).HasColumnType("datetime").HasColumnName("dateTimePayed");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.ShiftId).HasColumnName("shiftID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
        });

        modelBuilder.Entity<tbl_Expense>()
            .HasOne(td => td.Employee)
            .WithMany()
            .HasForeignKey(td => td.EmployeeId);

        modelBuilder.Entity<tbl_Expense>()
            .HasOne(td => td.Customer)
            .WithMany()
            .HasForeignKey(td => td.CustomerId);

        modelBuilder.Entity<tbl_Expense>()
            .HasOne(td => td.Supplier)
            .WithMany()
            .HasForeignKey(td => td.SupplierId);

        modelBuilder.Entity<tbl_Expense>()
            .HasOne(td => td.Shift)
            .WithMany()
            .HasForeignKey(td => td.ShiftId);

        modelBuilder.Entity<tbl_Expense>()
            .HasOne(td => td.ExpenseTypeData)
            .WithMany()
            .HasForeignKey(td => td.ExpenseType);

        modelBuilder.Entity<tbl_Location>(entity =>
        {
            entity.ToTable("tbl_location");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Location).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<tbl_OrderProcess>(entity =>
        {
            entity.ToTable("tbl_OrderProcesses");
            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.SortId).HasColumnName("SortID");
        });

        modelBuilder.Entity<tbl_OrderStatus>(entity =>
        {
            entity.ToTable("tbl_OrderStatus");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.OrderName).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.SortOrder).HasColumnName("sortOrder");
        });

        modelBuilder.Entity<tbl_Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_Payments_1");
            entity.ToTable("tbl_Payments");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.BankingDate).HasColumnType("datetime");
            entity.Property(e => e.CardRef).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.ChequeNo).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
            entity.Property(e => e.NameOnCheque).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.PaymentModeId).HasColumnName("PaymentModeID");
            entity.Property(e => e.SaleId).HasColumnName("saleID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierPaymentId).HasColumnName("SupplierPaymentID");
        });

        modelBuilder.Entity<tbl_PaymentAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tb_paymentAccounts");
            entity.ToTable("tbl_paymentAccounts");
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18, 4)").HasColumnName("openingBalance");
            entity.Property(e => e.PaymentAccountName).HasMaxLength(50).IsUnicode(false).HasColumnName("paymentAccountName");
            entity.Property(e => e.PaymentTypeId).HasColumnName("paymentTypeID");
        });

        modelBuilder.Entity<tbl_PaymentMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_payments");
            entity.ToTable("tbl_paymentMode");
            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("PaymentModeID");
            entity.Property(e => e.Description).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<tbl_Product>()
            .HasOne(e => e.Tax)
            .WithMany()
            .HasForeignKey(e => e.TaxId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<tbl_Product>(entity =>
        {
            entity.ToTable("tbl_products");
            entity.Property(e => e.BarCode).HasMaxLength(50).IsUnicode(false).HasColumnName("barCode");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.CompoundCostPricing).HasColumnName("compoundCostPricing");
            entity.Property(e => e.CostExclusive).HasColumnType("decimal(18, 4)").HasColumnName("costExclusive");
            entity.Property(e => e.CostIncStatus).HasColumnName("costIncStatus");
            entity.Property(e => e.CostInclusive).HasColumnType("decimal(18, 4)").HasColumnName("costInclusive");
            entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime").HasColumnName("createdDate");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.HasSubProduct).HasColumnName("hasSubProduct");
            entity.Property(e => e.InStock).HasColumnType("decimal(18, 4)").HasColumnName("inStock");
            entity.Property(e => e.IsAsubProduct).HasColumnName("isAsubProduct");
            entity.Property(e => e.Location).HasMaxLength(200).IsUnicode(false).HasColumnName("location");
            entity.Property(e => e.PriceExclusive).HasColumnType("decimal(18, 4)").HasColumnName("priceExclusive");
            entity.Property(e => e.PriceExclusive2).HasColumnType("decimal(18, 4)").HasColumnName("priceExclusive2");
            entity.Property(e => e.PriceInclusive).HasColumnType("decimal(18, 4)").HasColumnName("priceInclusive");
            entity.Property(e => e.PriceInclusive2).HasColumnType("decimal(18, 4)").HasColumnName("priceInclusive2");
            entity.Property(e => e.ProductCode).HasMaxLength(50).IsUnicode(false).HasColumnName("productCode");
            entity.Property(e => e.ProductImage).HasMaxLength(200).IsUnicode(false).HasColumnName("productImage");
            entity.Property(e => e.ProductName).HasMaxLength(150).IsUnicode(false).HasColumnName("productName");
            entity.Property(e => e.ReOrderLevel).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ReOrderQty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SegmentId).HasColumnName("segmentId");
            entity.Property(e => e.SupplierId).HasColumnName("supplierId");
            entity.Property(e => e.TaxId).HasColumnName("tax");
            entity.Property(e => e.TrackInventory).HasColumnName("trackInventory");
        });

        modelBuilder.Entity<tbl_ProductReceiving>(entity =>
        {
            entity.ToTable("tbl_ProductReceiving");
            entity.HasIndex(e => e.Id, "UQ__Received__DE143AD31592E241").IsUnique();
            entity.Property(e => e.CostExc).HasColumnType("decimal(18, 4)").HasColumnName("costExc");
            entity.Property(e => e.CostInc).HasColumnType("decimal(18, 4)").HasColumnName("costInc");
            entity.Property(e => e.CreditSupplierAcc).HasColumnName("creditSupplierAcc");
            entity.Property(e => e.DateReceived).HasColumnType("datetime");
            entity.Property(e => e.GrnsupplierNumber).HasMaxLength(50).IsUnicode(false).HasColumnName("GRNSupplierNumber");
            entity.Property(e => e.NewCostInc).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.NewPriceInc).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PriceChangeScheduled).HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SupplierAccount).HasColumnName("supplierAccount");
        });

        modelBuilder.Entity<tbl_ProductRelationship>(entity =>
        {
            entity.ToTable("tbl_ProductRelationships");
            entity.Property(e => e.HasAsubProductId).HasColumnName("hasAsubProductID");
            entity.Property(e => e.IsAsubProductId).HasColumnName("isAsubProductID");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 4)").HasColumnName("qty");
            entity.Property(e => e.SortOrder).HasColumnName("sortOrder");
        });

        modelBuilder.Entity<tbl_RoleValue>(entity =>
        {
            entity.ToTable("tbl_RoleValues");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<tbl_Segment>(entity =>
        {
            entity.ToTable("tbl_segment");
            entity.Property(e => e.Description).HasMaxLength(100).IsUnicode(false).HasColumnName("description");
            entity.Property(e => e.HideInPos).HasColumnName("hideInPOS");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Segment).HasMaxLength(100).IsUnicode(false).HasColumnName("segment");
        });

        modelBuilder.Entity<tbl_Shift>(entity =>
        {
            entity.ToTable("tbl_shifts");
            entity.Property(e => e.ActiveId).HasColumnName("activeId");
            entity.Property(e => e.Comment).HasMaxLength(150).IsUnicode(false).HasColumnName("comment");
            entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18, 4)").HasColumnName("currentBalance");
            entity.Property(e => e.DateTimeClosed).HasColumnType("datetime").HasColumnName("dateTimeClosed");
            entity.Property(e => e.DateTimeOpened).HasColumnType("datetime").HasColumnName("dateTimeOpened");
            entity.Property(e => e.DrawerStatus).HasColumnName("drawerStatus");
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18, 4)").HasColumnName("openingBalance");
            entity.Property(e => e.ShiftEndAcc).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndAcc");
            entity.Property(e => e.ShiftEndAmount).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndAmount");
            entity.Property(e => e.ShiftEndBank).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndBank");
            entity.Property(e => e.ShiftEndCard).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndCard");
            entity.Property(e => e.ShiftEndCash).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndCash");
            entity.Property(e => e.ShiftEndCheque).HasColumnType("decimal(18, 4)").HasColumnName("shiftEndCheque");
            entity.Property(e => e.SubActiveId).HasColumnName("subActiveId");
            entity.Property(e => e.UserId).HasColumnName("userId");
        });

        modelBuilder.Entity<tbl_Size>(entity =>
        {
            entity.ToTable("tbl_Sizes");
            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("SizeID");
        });

        modelBuilder.Entity<tbl_SlipLayout>(entity =>
        {
            entity.ToTable("tbl_SlipLayout");
        });

        modelBuilder.Entity<tbl_Supplier>(entity =>
        {
            entity.ToTable("tbl_Supplier");
            entity.Property(e => e.AccountNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.CardNumber).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Company).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Contact).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 4)").HasColumnName("creditLimit");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.VatNumber).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<tbl_SupplierPayment>(entity =>
        {
            entity.ToTable("tbl_SupplierPayment");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.DateTimePayed).HasColumnType("datetime").HasColumnName("dateTimePayed");
            entity.Property(e => e.PaymentId).HasColumnName("paymentID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.UserId).HasColumnName("userID");
        });

        modelBuilder.Entity<tbl_Tax>(entity =>
        {
            entity.ToTable("tbl_tax");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.TaxDescription).HasMaxLength(150).IsUnicode(false).HasColumnName("taxDescription");
            entity.Property(e => e.TaxValue).HasColumnType("decimal(18, 4)").HasColumnName("taxValue");
        });

        modelBuilder.Entity<tbl_Transaction>(entity =>
        {
            entity.ToTable("tbl_transaction");
            entity.Property(e => e.Change).HasColumnType("decimal(18, 2)").HasColumnName("change");
            entity.Property(e => e.CustomerId).HasColumnName("customerId");
            entity.Property(e => e.OrderStatus).HasColumnName("orderStatus");
            entity.Property(e => e.QuotationId).HasColumnName("quotationID");
            entity.Property(e => e.SaleAgentId).HasColumnName("saleAgentID");
            entity.Property(e => e.SaleTotal).HasColumnType("decimal(18, 2)").HasColumnName("saleTotal");
            entity.Property(e => e.ShiftId).HasColumnName("shiftId");
            entity.Property(e => e.SoldBy).HasColumnName("soldBy");
            entity.Property(e => e.TransactionComment).HasMaxLength(300).IsUnicode(false).HasColumnName("transactionComment");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime").HasColumnName("transactionDate");
            entity.Property(e => e.TransactionStatus).HasColumnName("transactionStatus");
        });

        modelBuilder.Entity<tbl_TransactionDetail>(entity =>
        {
            entity.ToTable("tbl_transactionDetail");
            entity.Property(e => e.CostExc).HasColumnType("decimal(18, 2)").HasColumnName("costExc");
            entity.Property(e => e.CostInc).HasColumnType("decimal(18, 2)").HasColumnName("costInc");
            entity.Property(e => e.CostIncState).HasColumnName("costIncState");
            entity.Property(e => e.DiscountId).HasColumnName("discountID");
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(18, 2)").HasColumnName("discountPercent");
            entity.Property(e => e.PriceExc).HasColumnType("decimal(18, 2)").HasColumnName("priceExc");
            entity.Property(e => e.PriceInc).HasColumnType("decimal(18, 2)").HasColumnName("priceInc");
            entity.Property(e => e.ProductId).HasColumnName("productID");
            entity.Property(e => e.Qty).HasColumnType("decimal(18, 2)").HasColumnName("qty");
            entity.Property(e => e.SortOrder).HasColumnName("sortOrder");
            entity.Property(e => e.SpecialPricingUsed).HasColumnName("specialPricingUsed");
            entity.Property(e => e.TaxId).HasColumnName("taxID");
            entity.Property(e => e.TaxPercent).HasColumnType("decimal(18, 2)").HasColumnName("taxPercent");
            entity.Property(e => e.TotalPriceExc).HasColumnType("decimal(18, 2)").HasColumnName("totalPriceExc");
            entity.Property(e => e.TotalPriceInc).HasColumnType("decimal(18, 2)").HasColumnName("totalPriceInc");
            entity.Property(e => e.TransactionId).HasColumnName("transactionID");
        });

        modelBuilder.Entity<tbl_UniqueField>(entity =>
        {
            entity.ToTable("tbl_UniqueFields");
            entity.Property(e => e.UniqueField).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<tbl_ExpenseType>(entity =>
        {
            entity.ToTable("tbl_ExpenseType");
            entity.Property(e => e.Description).HasMaxLength(150).IsUnicode(false);
        });

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

    //private void UpdateTimestamps()
    //{
    //    var entities = ChangeTracker.Entries<BaseEntity>();
    //    foreach (var entity in entities)
    //    {
    //        if (entity.State == EntityState.Added)
    //        {
    //            entity.Entity.DateTimeCreated = DateTime.UtcNow;
    //            entity.Entity.DateTimeModified = DateTime.UtcNow;
    //            entity.Entity.LastModifiedBy = _userId;
    //            entity.Entity.TenantId = entity.Entity.TenantId ?? _tenantId;
    //            entity.Entity.Id = !string.IsNullOrEmpty(entity.Entity.Id) ? entity.Entity.Id: Guid.NewGuid().ToString();

    //        }
    //        else if (entity.State == EntityState.Modified)
    //        {
    //            entity.Entity.DateTimeModified = DateTime.UtcNow;
    //            entity.Property(p => p.DateTimeCreated).IsModified = false;
    //            entity.Entity.LastModifiedBy = _userId;
    //        }
    //    }
    //}

}