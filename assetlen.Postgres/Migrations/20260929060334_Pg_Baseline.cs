using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace assetlen.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Pg_Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Aboutme = table.Column<string>(type: "text", nullable: true),
                    Industry = table.Column<string>(type: "text", nullable: true),
                    Contacts = table.Column<string>(type: "text", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "text", nullable: true),
                    CoverPhotoUrl = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true),
                    IsEmployee = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExtractionRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StartedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Engine = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MessagesRead = table.Column<int>(type: "integer", nullable: false),
                    ProposalsCreated = table.Column<int>(type: "integer", nullable: false),
                    ReadingsCreated = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExtractionRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Logs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    MessageTemplate = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ShiftId = table.Column<string>(type: "text", nullable: true),
                    SaleId = table.Column<string>(type: "text", nullable: true),
                    LogTypeId = table.Column<int>(type: "integer", nullable: true),
                    OldQty = table.Column<int>(type: "integer", nullable: true),
                    NewQty = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_RoleValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: true),
                    RoleID = table.Column<int>(type: "integer", nullable: true),
                    RoleValue = table.Column<bool>(type: "boolean", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RoleValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SubscriptionRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OrganisationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    ContactPersonName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    RequestedSeats = table.Column<int>(type: "integer", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SubmittedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SubmittedByEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    QuotedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    QuoteCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    QuoteNotes = table.Column<string>(type: "text", nullable: true),
                    QuotedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuotedByUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    QuotedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedByUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SubscriptionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SyncLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserJwt = table.Column<string>(type: "text", nullable: true),
                    Method = table.Column<string>(type: "text", nullable: true),
                    Endpoint = table.Column<string>(type: "text", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CocurrencyKey = table.Column<string>(type: "text", nullable: true),
                    keyHarsh = table.Column<string>(type: "text", nullable: true),
                    LastRenewal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BussinessRegNumber = table.Column<string>(type: "text", nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Industry = table.Column<string>(type: "text", nullable: true),
                    NumberOfEmployees = table.Column<int>(type: "integer", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "numeric", nullable: true),
                    CEO = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: true),
                    StockSymbol = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ResendCount = table.Column<int>(type: "integer", nullable: false),
                    LastResentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResetToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    BrowserType = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DeviceFingerprint = table.Column<string>(type: "text", nullable: false),
                    FirstLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LoginCount = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_EmployeeApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TargetUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ApproverUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ApproverUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_EmployeeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_EmployeeApprovals_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Projects_RS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TotalBudget = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ExpectedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevisedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvestorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ProjectManagerId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OwnerTenantId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FloorAreaSqm = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    SizeTier = table.Column<int>(type: "integer", nullable: false),
                    SizeSource = table.Column<int>(type: "integer", nullable: false),
                    SizeTierConfirmedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    SizeTierConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IngestEmailKey = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ReportDraftingEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsFirstFreeProject = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubscriptionActive = table.Column<bool>(type: "boolean", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ParentProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Projects_RS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Projects_RS_AspNetUsers_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Projects_RS_AspNetUsers_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Projects_RS_tbl_Projects_RS_ParentProjectId",
                        column: x => x.ParentProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EndpointHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    P256dh = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Auth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    LastSuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PushSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PushSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_TenantMemberships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    TenantId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Roles = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_TenantMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_TenantMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SubscriptionSeats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RequestId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedUserId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SubscriptionSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SubscriptionSeats_tbl_SubscriptionRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "tbl_SubscriptionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Configuration",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StringValue = table.Column<string>(type: "text", nullable: true),
                    SettingID = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Configuration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Artifacts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ByteSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    UploadedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Artifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Artifacts_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Artifacts_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_BriefPublications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Day = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    PublishedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    FramesExposed = table.Column<int>(type: "integer", nullable: false),
                    FramesDropped = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_BriefPublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_BriefPublications_AspNetUsers_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_BriefPublications_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrentRevisionId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Documents_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    PartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    IsMediator = table.Column<bool>(type: "boolean", nullable: false),
                    Specialization = table.Column<int>(type: "integer", nullable: false),
                    HandlesMoney = table.Column<bool>(type: "boolean", nullable: true),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProjectMembers_AspNetUsers_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectMembers_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProjectPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    PinnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProjectPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProjectPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectPreferences_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProjectSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    InvestorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProjectSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProjectSubscriptions_AspNetUsers_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectSubscriptions_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Stages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ParentStageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CatalogueKey = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    BaselineStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BaselineEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Stages_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Stages_tbl_Stages_ParentStageId",
                        column: x => x.ParentStageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_WorksReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindowFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Audience = table.Column<int>(type: "integer", nullable: false),
                    IssuedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssueKind = table.Column<int>(type: "integer", nullable: false),
                    IssueReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TriggerKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CoveringNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "text", nullable: true),
                    NarrativeJson = table.Column<string>(type: "text", nullable: true),
                    ContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PreviousReportId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliveryNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_WorksReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_WorksReports_AspNetUsers_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_WorksReports_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_PushDeliveries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SubscriptionId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Body = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HttpStatus = table.Column<int>(type: "integer", nullable: true),
                    Error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PushDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PushDeliveries_tbl_PushSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "tbl_PushSubscriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactPosters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    PosterArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DurationSeconds = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactPosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactPosters_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactPosters_tbl_Artifacts_PosterArtifactId",
                        column: x => x.PosterArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactRefs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ExposedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ExposedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactRefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRefs_AspNetUsers_ExposedById",
                        column: x => x.ExposedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRefs_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactTexts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Engine = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    CharCount = table.Column<int>(type: "integer", nullable: false),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactTexts_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_IngestBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ArchiveArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    ImportedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ImportedSide = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ParsedMessageCount = table.Column<int>(type: "integer", nullable: false),
                    ImportedMessageCount = table.Column<int>(type: "integer", nullable: false),
                    DuplicateMessageCount = table.Column<int>(type: "integer", nullable: false),
                    MediaMessageCount = table.Column<int>(type: "integer", nullable: false),
                    NewArtifactCount = table.Column<int>(type: "integer", nullable: false),
                    DuplicateArtifactCount = table.Column<int>(type: "integer", nullable: false),
                    UnmatchedMediaCount = table.Column<int>(type: "integer", nullable: false),
                    BoundMediaCount = table.Column<int>(type: "integer", nullable: false),
                    ParticipantCount = table.Column<int>(type: "integer", nullable: false),
                    FirstMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_IngestBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_AspNetUsers_ImportedById",
                        column: x => x.ImportedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_tbl_Artifacts_ArchiveArtifactId",
                        column: x => x.ArchiveArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactRevisions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    RevisionNo = table.Column<int>(type: "integer", nullable: false),
                    IssuedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupersededByRevisionId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_AspNetUsers_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_tbl_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "tbl_Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_BudgetLineItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_BudgetLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_BudgetLineItems_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_BudgetLineItems_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_BudgetLineItems_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Deliverables",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Deliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_AspNetUsers_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_FundingEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ConfirmedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeclaredCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DeclaredAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ReceiptNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EvidenceFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    SettledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SettledById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FundingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_AspNetUsers_ConfirmedById",
                        column: x => x.ConfirmedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_AspNetUsers_PaidById",
                        column: x => x.PaidById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressReadings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceKind = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceSide = table.Column<int>(type: "integer", nullable: true),
                    SourceImportedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressReadings_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_StageClaims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvidenceArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClearedAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClearedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    QueryNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_StageClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_AspNetUsers_ClaimedById",
                        column: x => x.ClaimedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_AspNetUsers_ClearedById",
                        column: x => x.ClearedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_IngestedMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BatchId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ExternalAuthor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AuthorMemberId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Body = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    MediaFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    IsSystemMessage = table.Column<bool>(type: "boolean", nullable: false),
                    SequenceNo = table.Column<int>(type: "integer", nullable: false),
                    DedupeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_IngestedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_IngestBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "tbl_IngestBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_ProjectMembers_AuthorMemberId",
                        column: x => x.AuthorMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Receipts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    BudgetLineItemId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReceiptImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Receipts_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Receipts_tbl_BudgetLineItems_BudgetLineItemId",
                        column: x => x.BudgetLineItemId,
                        principalTable: "tbl_BudgetLineItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Commitments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Maturity = table.Column<int>(type: "integer", nullable: false),
                    QueryState = table.Column<int>(type: "integer", nullable: false),
                    SourceChannel = table.Column<int>(type: "integer", nullable: false),
                    AccountableMemberId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AgreedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AgreedWithMemberId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    AgreedWithPartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AgreedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RecordedBySide = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    DependsOnStageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OwedBySide = table.Column<int>(type: "integer", nullable: true),
                    SupersedesId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupersededById = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CounterpartyConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CounterpartyConfirmedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DisputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisputedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DisputeNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolutionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IngestedMessageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Commitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_AspNetUsers_AgreedById",
                        column: x => x.AgreedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_AspNetUsers_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Commitments_SupersedesId",
                        column: x => x.SupersedesId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_ProjectMembers_AccountableMemberId",
                        column: x => x.AccountableMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_ProjectMembers_AgreedWithMemberId",
                        column: x => x.AgreedWithMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Stages_DependsOnStageId",
                        column: x => x.DependsOnStageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressUpdates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    HasIssues = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "integer", nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    DeliverableId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ClientCaptureId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VoiceArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Artifacts_VoiceArtifactId",
                        column: x => x.VoiceArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExtractionProposals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    RunId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Maturity = table.Column<int>(type: "integer", nullable: false),
                    OwedBySide = table.Column<int>(type: "integer", nullable: true),
                    PartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Rule = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Engine = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Contested = table.Column<bool>(type: "boolean", nullable: false),
                    ContestNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecidedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FlagId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceSide = table.Column<int>(type: "integer", nullable: false),
                    SourceImportedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    SourceSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExtractionProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_tbl_IngestedMessages_IngestedMessag~",
                        column: x => x.IngestedMessageId,
                        principalTable: "tbl_IngestedMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_MediaBindings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    BatchId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    StampedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MediaBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MediaBindings_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_MediaBindings_tbl_IngestedMessages_IngestedMessageId",
                        column: x => x.IngestedMessageId,
                        principalTable: "tbl_IngestedMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Annotations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LayerId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AuthorSide = table.Column<int>(type: "integer", nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    ShapesJson = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Annotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_CommitmentEstimates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    RecordedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CommitmentEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentEstimates_AspNetUsers_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentEstimates_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_CommitmentLinks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Relation = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CommitmentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentLinks_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentLinks_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Variations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CostDelta = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    TimeDeltaDays = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RaisedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Variations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Variations_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_AspNetUsers_RaisedById",
                        column: x => x.RaisedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    ExposedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ExposedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Curation = table.Column<int>(type: "integer", nullable: false),
                    CuratedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CuratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressImages_AspNetUsers_ExposedById",
                        column: x => x.ExposedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressImages_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressImages_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ClaimEvidence",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ClaimId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ProgressImageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProgressReadingId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ClaimEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_ProgressImages_ProgressImageId",
                        column: x => x.ProgressImageId,
                        principalTable: "tbl_ProgressImages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_ProgressReadings_ProgressReadingId",
                        column: x => x.ProgressReadingId,
                        principalTable: "tbl_ProgressReadings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_StageClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "tbl_StageClaims",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Flags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProgressUpdateId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProgressImageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AssignedToId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ResolvedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNudgeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsNudgeArchived = table.Column<bool>(type: "boolean", nullable: false),
                    CommitmentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OwnerMemberId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    OwnerPartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Flags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Flags_AspNetUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_AspNetUsers_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_ProgressImages_ProgressImageId",
                        column: x => x.ProgressImageId,
                        principalTable: "tbl_ProgressImages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_ProjectMembers_OwnerMemberId",
                        column: x => x.OwnerMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Flags_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProgressImageId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CommentText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ParentCommentId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "tbl_ProgressComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressImages_ProgressImageId",
                        column: x => x.ProgressImageId,
                        principalTable: "tbl_ProgressImages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DateTimeCreated",
                table: "AspNetUsers",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DateTimeModified",
                table: "AspNetUsers",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted",
                table: "AspNetUsers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastModifiedBy",
                table: "AspNetUsers",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_DateTimeCreated",
                table: "RefreshTokens",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_DateTimeModified",
                table: "RefreshTokens",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsDeleted",
                table: "RefreshTokens",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_LastModifiedBy",
                table: "RefreshTokens",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RefreshToken_deviceFingerprint",
                table: "RefreshTokens",
                column: "DeviceFingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RefreshToken_token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_Annotation_CommitmentId",
                table: "tbl_Annotations",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotation_ProjectId",
                table: "tbl_Annotations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_AuthorId",
                table: "tbl_Annotations",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_DateTimeCreated",
                table: "tbl_Annotations",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_DateTimeModified",
                table: "tbl_Annotations",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_IsDeleted",
                table: "tbl_Annotations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_LastModifiedBy",
                table: "tbl_Annotations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_Annotation_Artifact_Layer_Version",
                table: "tbl_Annotations",
                columns: new[] { "ArtifactId", "LayerId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_DateTimeCreated",
                table: "tbl_ArtifactPosters",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_DateTimeModified",
                table: "tbl_ArtifactPosters",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_IsDeleted",
                table: "tbl_ArtifactPosters",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_LastModifiedBy",
                table: "tbl_ArtifactPosters",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_PosterArtifactId",
                table: "tbl_ArtifactPosters",
                column: "PosterArtifactId");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactPoster_ArtifactId",
                table: "tbl_ArtifactPosters",
                column: "ArtifactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_ArtifactId",
                table: "tbl_ArtifactRefs",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_Project_Channel",
                table: "tbl_ArtifactRefs",
                columns: new[] { "ProjectId", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_Target",
                table: "tbl_ArtifactRefs",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_DateTimeCreated",
                table: "tbl_ArtifactRefs",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_DateTimeModified",
                table: "tbl_ArtifactRefs",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_ExposedById",
                table: "tbl_ArtifactRefs",
                column: "ExposedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_IsDeleted",
                table: "tbl_ArtifactRefs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_LastModifiedBy",
                table: "tbl_ArtifactRefs",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactRef_Artifact_Target",
                table: "tbl_ArtifactRefs",
                columns: new[] { "ArtifactId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRevision_DocumentId",
                table: "tbl_ArtifactRevisions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_ArtifactId",
                table: "tbl_ArtifactRevisions",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_DateTimeCreated",
                table: "tbl_ArtifactRevisions",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_DateTimeModified",
                table: "tbl_ArtifactRevisions",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_IsDeleted",
                table: "tbl_ArtifactRevisions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_IssuedById",
                table: "tbl_ArtifactRevisions",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_LastModifiedBy",
                table: "tbl_ArtifactRevisions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactRevision_Document_RevisionNo",
                table: "tbl_ArtifactRevisions",
                columns: new[] { "DocumentId", "RevisionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_ProjectId",
                table: "tbl_Artifacts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_DateTimeCreated",
                table: "tbl_Artifacts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_DateTimeModified",
                table: "tbl_Artifacts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_IsDeleted",
                table: "tbl_Artifacts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_LastModifiedBy",
                table: "tbl_Artifacts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_UploadedById",
                table: "tbl_Artifacts",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "UX_Artifact_Project_Sha256",
                table: "tbl_Artifacts",
                columns: new[] { "ProjectId", "Sha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactText_Project_Status",
                table: "tbl_ArtifactTexts",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactText_Text_trgm",
                table: "tbl_ArtifactTexts",
                column: "Text")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_DateTimeCreated",
                table: "tbl_ArtifactTexts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_DateTimeModified",
                table: "tbl_ArtifactTexts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_IsDeleted",
                table: "tbl_ArtifactTexts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_LastModifiedBy",
                table: "tbl_ArtifactTexts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactText_ArtifactId",
                table: "tbl_ArtifactTexts",
                column: "ArtifactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_DateTimeCreated",
                table: "tbl_BriefPublications",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_DateTimeModified",
                table: "tbl_BriefPublications",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_IsDeleted",
                table: "tbl_BriefPublications",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_LastModifiedBy",
                table: "tbl_BriefPublications",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_PublishedById",
                table: "tbl_BriefPublications",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "UX_BriefPublication_Project_Day",
                table: "tbl_BriefPublications",
                columns: new[] { "ProjectId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLineItem_Category",
                table: "tbl_BudgetLineItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLineItem_ProjectId",
                table: "tbl_BudgetLineItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLineItem_StageId",
                table: "tbl_BudgetLineItems",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BudgetLineItems_CreatedById",
                table: "tbl_BudgetLineItems",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BudgetLineItems_DateTimeCreated",
                table: "tbl_BudgetLineItems",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BudgetLineItems_DateTimeModified",
                table: "tbl_BudgetLineItems",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BudgetLineItems_IsDeleted",
                table: "tbl_BudgetLineItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BudgetLineItems_LastModifiedBy",
                table: "tbl_BudgetLineItems",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimEvidence_ClaimId",
                table: "tbl_ClaimEvidence",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ArtifactId",
                table: "tbl_ClaimEvidence",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DateTimeCreated",
                table: "tbl_ClaimEvidence",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DateTimeModified",
                table: "tbl_ClaimEvidence",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DeliverableId",
                table: "tbl_ClaimEvidence",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_IsDeleted",
                table: "tbl_ClaimEvidence",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_LastModifiedBy",
                table: "tbl_ClaimEvidence",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ProgressImageId",
                table: "tbl_ClaimEvidence",
                column: "ProgressImageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ProgressReadingId",
                table: "tbl_ClaimEvidence",
                column: "ProgressReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentEstimate_CommitmentId",
                table: "tbl_CommitmentEstimates",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_DateTimeCreated",
                table: "tbl_CommitmentEstimates",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_DateTimeModified",
                table: "tbl_CommitmentEstimates",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_IsDeleted",
                table: "tbl_CommitmentEstimates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_LastModifiedBy",
                table: "tbl_CommitmentEstimates",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_RecordedById",
                table: "tbl_CommitmentEstimates",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentLink_CommitmentId",
                table: "tbl_CommitmentLinks",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentLink_Target",
                table: "tbl_CommitmentLinks",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_DateTimeCreated",
                table: "tbl_CommitmentLinks",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_DateTimeModified",
                table: "tbl_CommitmentLinks",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_IsDeleted",
                table: "tbl_CommitmentLinks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_LastModifiedBy",
                table: "tbl_CommitmentLinks",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_ProjectId",
                table: "tbl_CommitmentLinks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "UX_CommitmentLink_Commitment_Target_Relation",
                table: "tbl_CommitmentLinks",
                columns: new[] { "CommitmentId", "TargetType", "TargetId", "Relation" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_Body_trgm",
                table: "tbl_Commitments",
                column: "Body")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_DeliverableId",
                table: "tbl_Commitments",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_Project_Accountable",
                table: "tbl_Commitments",
                columns: new[] { "ProjectId", "AccountableMemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_ProjectId",
                table: "tbl_Commitments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_StageId",
                table: "tbl_Commitments",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_SupersedesId",
                table: "tbl_Commitments",
                column: "SupersedesId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_Title_trgm",
                table: "tbl_Commitments",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AccountableMemberId",
                table: "tbl_Commitments",
                column: "AccountableMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AgreedById",
                table: "tbl_Commitments",
                column: "AgreedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AgreedWithMemberId",
                table: "tbl_Commitments",
                column: "AgreedWithMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DateTimeCreated",
                table: "tbl_Commitments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DateTimeModified",
                table: "tbl_Commitments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DependsOnStageId",
                table: "tbl_Commitments",
                column: "DependsOnStageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_IsDeleted",
                table: "tbl_Commitments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_LastModifiedBy",
                table: "tbl_Commitments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_RecordedById",
                table: "tbl_Commitments",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_DateTimeCreated",
                table: "tbl_Configuration",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_DateTimeModified",
                table: "tbl_Configuration",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_IsDeleted",
                table: "tbl_Configuration",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_LastModifiedBy",
                table: "tbl_Configuration",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_TenantId",
                table: "tbl_Configuration",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverable_ProjectId",
                table: "tbl_Deliverables",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverable_Stage_Order",
                table: "tbl_Deliverables",
                columns: new[] { "StageId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_CompletedById",
                table: "tbl_Deliverables",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_DateTimeCreated",
                table: "tbl_Deliverables",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_DateTimeModified",
                table: "tbl_Deliverables",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_IsDeleted",
                table: "tbl_Deliverables",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_LastModifiedBy",
                table: "tbl_Deliverables",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Document_Kind",
                table: "tbl_Documents",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ProjectId",
                table: "tbl_Documents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_DateTimeCreated",
                table: "tbl_Documents",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_DateTimeModified",
                table: "tbl_Documents",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_IsDeleted",
                table: "tbl_Documents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_LastModifiedBy",
                table: "tbl_Documents",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApproval_TargetUser_Approver",
                table: "tbl_EmployeeApprovals",
                columns: new[] { "TargetUserId", "ApproverUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApproval_TargetUserId",
                table: "tbl_EmployeeApprovals",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApprovals_DateTimeCreated",
                table: "tbl_EmployeeApprovals",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApprovals_DateTimeModified",
                table: "tbl_EmployeeApprovals",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApprovals_IsDeleted",
                table: "tbl_EmployeeApprovals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_EmployeeApprovals_LastModifiedBy",
                table: "tbl_EmployeeApprovals",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionProposal_MessageId",
                table: "tbl_ExtractionProposals",
                column: "IngestedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionProposal_Project_Status",
                table: "tbl_ExtractionProposals",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DateTimeCreated",
                table: "tbl_ExtractionProposals",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DateTimeModified",
                table: "tbl_ExtractionProposals",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DecidedById",
                table: "tbl_ExtractionProposals",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_IsDeleted",
                table: "tbl_ExtractionProposals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_LastModifiedBy",
                table: "tbl_ExtractionProposals",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_StageId",
                table: "tbl_ExtractionProposals",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "UX_ExtractionProposal_Project_Fingerprint",
                table: "tbl_ExtractionProposals",
                columns: new[] { "ProjectId", "Fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionRun_ProjectId",
                table: "tbl_ExtractionRuns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_DateTimeCreated",
                table: "tbl_ExtractionRuns",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_DateTimeModified",
                table: "tbl_ExtractionRuns",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_IsDeleted",
                table: "tbl_ExtractionRuns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_LastModifiedBy",
                table: "tbl_ExtractionRuns",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_AssignedToId",
                table: "tbl_Flags",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_CommitmentId",
                table: "tbl_Flags",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_ProgressImageId",
                table: "tbl_Flags",
                column: "ProgressImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_ProgressUpdateId",
                table: "tbl_Flags",
                column: "ProgressUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_ProjectId",
                table: "tbl_Flags",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_StageId",
                table: "tbl_Flags",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Flag_Status",
                table: "tbl_Flags",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_CreatedById",
                table: "tbl_Flags",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_DateTimeCreated",
                table: "tbl_Flags",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_DateTimeModified",
                table: "tbl_Flags",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_IsDeleted",
                table: "tbl_Flags",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_LastModifiedBy",
                table: "tbl_Flags",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_OwnerMemberId",
                table: "tbl_Flags",
                column: "OwnerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_ResolvedById",
                table: "tbl_Flags",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_FundingEntry_ProjectId",
                table: "tbl_FundingEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingEntry_StageId",
                table: "tbl_FundingEntries",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingEntry_Status",
                table: "tbl_FundingEntries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_ConfirmedById",
                table: "tbl_FundingEntries",
                column: "ConfirmedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_DateTimeCreated",
                table: "tbl_FundingEntries",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_DateTimeModified",
                table: "tbl_FundingEntries",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_IsDeleted",
                table: "tbl_FundingEntries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_LastModifiedBy",
                table: "tbl_FundingEntries",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FundingEntries_PaidById",
                table: "tbl_FundingEntries",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_IngestBatch_Project_Source",
                table: "tbl_IngestBatches",
                columns: new[] { "ProjectId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_IngestBatch_ProjectId",
                table: "tbl_IngestBatches",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_ArchiveArtifactId",
                table: "tbl_IngestBatches",
                column: "ArchiveArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_DateTimeCreated",
                table: "tbl_IngestBatches",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_DateTimeModified",
                table: "tbl_IngestBatches",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_ImportedById",
                table: "tbl_IngestBatches",
                column: "ImportedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_IsDeleted",
                table: "tbl_IngestBatches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_LastModifiedBy",
                table: "tbl_IngestBatches",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IngestedMessage_BatchId",
                table: "tbl_IngestedMessages",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestedMessage_Body_trgm",
                table: "tbl_IngestedMessages",
                column: "Body")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_IngestedMessage_Project_SentAt",
                table: "tbl_IngestedMessages",
                columns: new[] { "ProjectId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_ArtifactId",
                table: "tbl_IngestedMessages",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_AuthorMemberId",
                table: "tbl_IngestedMessages",
                column: "AuthorMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_DateTimeCreated",
                table: "tbl_IngestedMessages",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_DateTimeModified",
                table: "tbl_IngestedMessages",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_IsDeleted",
                table: "tbl_IngestedMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_LastModifiedBy",
                table: "tbl_IngestedMessages",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_IngestedMessage_Project_DedupeKey",
                table: "tbl_IngestedMessages",
                columns: new[] { "ProjectId", "DedupeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Logs_DateTimeCreated",
                table: "tbl_Logs",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Logs_DateTimeModified",
                table: "tbl_Logs",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Logs_IsDeleted",
                table: "tbl_Logs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Logs_LastModifiedBy",
                table: "tbl_Logs",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MediaBinding_Project_Artifact",
                table: "tbl_MediaBindings",
                columns: new[] { "ProjectId", "ArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_ArtifactId",
                table: "tbl_MediaBindings",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_DateTimeCreated",
                table: "tbl_MediaBindings",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_DateTimeModified",
                table: "tbl_MediaBindings",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_IsDeleted",
                table: "tbl_MediaBindings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_LastModifiedBy",
                table: "tbl_MediaBindings",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_MediaBinding_MessageId",
                table: "tbl_MediaBindings",
                column: "IngestedMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComment_ImageId",
                table: "tbl_ProgressComments",
                column: "ProgressImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressComment_UpdateId",
                table: "tbl_ProgressComments",
                column: "ProgressUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_AuthorId",
                table: "tbl_ProgressComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_DateTimeCreated",
                table: "tbl_ProgressComments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_DateTimeModified",
                table: "tbl_ProgressComments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_IsDeleted",
                table: "tbl_ProgressComments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_LastModifiedBy",
                table: "tbl_ProgressComments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressComments_ParentCommentId",
                table: "tbl_ProgressComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressImage_UpdateId",
                table: "tbl_ProgressImages",
                column: "ProgressUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_ArtifactId",
                table: "tbl_ProgressImages",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_DateTimeCreated",
                table: "tbl_ProgressImages",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_DateTimeModified",
                table: "tbl_ProgressImages",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_ExposedById",
                table: "tbl_ProgressImages",
                column: "ExposedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_IsDeleted",
                table: "tbl_ProgressImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_LastModifiedBy",
                table: "tbl_ProgressImages",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReading_Project_ObservedAt",
                table: "tbl_ProgressReadings",
                columns: new[] { "ProjectId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReading_Stage_ObservedAt",
                table: "tbl_ProgressReadings",
                columns: new[] { "StageId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_DateTimeCreated",
                table: "tbl_ProgressReadings",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_DateTimeModified",
                table: "tbl_ProgressReadings",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_IsDeleted",
                table: "tbl_ProgressReadings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_LastModifiedBy",
                table: "tbl_ProgressReadings",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_CreatedAt",
                table: "tbl_ProgressUpdates",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_DeliverableId",
                table: "tbl_ProgressUpdates",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_Description_trgm",
                table: "tbl_ProgressUpdates",
                column: "Description")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_ProjectId",
                table: "tbl_ProgressUpdates",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_StageId",
                table: "tbl_ProgressUpdates",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_CreatedById",
                table: "tbl_ProgressUpdates",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_DateTimeModified",
                table: "tbl_ProgressUpdates",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_IsDeleted",
                table: "tbl_ProgressUpdates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_LastModifiedBy",
                table: "tbl_ProgressUpdates",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_VoiceArtifactId",
                table: "tbl_ProgressUpdates",
                column: "VoiceArtifactId");

            migrationBuilder.CreateIndex(
                name: "UX_ProgressUpdate_Project_ClientCapture",
                table: "tbl_ProgressUpdates",
                columns: new[] { "ProjectId", "ClientCaptureId" },
                unique: true,
                filter: "\"ClientCaptureId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_Project_Mediator",
                table: "tbl_ProjectMembers",
                columns: new[] { "ProjectId", "IsMediator" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_Project_Side",
                table: "tbl_ProjectMembers",
                columns: new[] { "ProjectId", "Side" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_Project_User",
                table: "tbl_ProjectMembers",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_ProjectId",
                table: "tbl_ProjectMembers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_UserId",
                table: "tbl_ProjectMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectMembers_AssignedById",
                table: "tbl_ProjectMembers",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectMembers_DateTimeCreated",
                table: "tbl_ProjectMembers",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectMembers_DateTimeModified",
                table: "tbl_ProjectMembers",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectMembers_IsDeleted",
                table: "tbl_ProjectMembers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectMembers_LastModifiedBy",
                table: "tbl_ProjectMembers",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPreference_User_Pinned",
                table: "tbl_ProjectPreferences",
                columns: new[] { "UserId", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPreference_User_Project",
                table: "tbl_ProjectPreferences",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_DateTimeCreated",
                table: "tbl_ProjectPreferences",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_DateTimeModified",
                table: "tbl_ProjectPreferences",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_IsDeleted",
                table: "tbl_ProjectPreferences",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_LastModifiedBy",
                table: "tbl_ProjectPreferences",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_ProjectId",
                table: "tbl_ProjectPreferences",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ArchivedAt",
                table: "tbl_Projects_RS",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Project_InvestorId",
                table: "tbl_Projects_RS",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_OwnerTenantId",
                table: "tbl_Projects_RS",
                column: "OwnerTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ParentProjectId",
                table: "tbl_Projects_RS",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectManagerId",
                table: "tbl_Projects_RS",
                column: "ProjectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_SizeTier",
                table: "tbl_Projects_RS",
                column: "SizeTier");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Status",
                table: "tbl_Projects_RS",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Projects_RS_DateTimeCreated",
                table: "tbl_Projects_RS",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Projects_RS_DateTimeModified",
                table: "tbl_Projects_RS",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Projects_RS_IsDeleted",
                table: "tbl_Projects_RS",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Projects_RS_LastModifiedBy",
                table: "tbl_Projects_RS",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSub_InvestorId",
                table: "tbl_ProjectSubscriptions",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSub_ProjectId",
                table: "tbl_ProjectSubscriptions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectSubscriptions_DateTimeCreated",
                table: "tbl_ProjectSubscriptions",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectSubscriptions_DateTimeModified",
                table: "tbl_ProjectSubscriptions",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectSubscriptions_IsDeleted",
                table: "tbl_ProjectSubscriptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectSubscriptions_LastModifiedBy",
                table: "tbl_ProjectSubscriptions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PushDelivery_Status",
                table: "tbl_PushDeliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PushDelivery_User_QueuedAt",
                table: "tbl_PushDeliveries",
                columns: new[] { "UserId", "QueuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_DateTimeCreated",
                table: "tbl_PushDeliveries",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_DateTimeModified",
                table: "tbl_PushDeliveries",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_IsDeleted",
                table: "tbl_PushDeliveries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_LastModifiedBy",
                table: "tbl_PushDeliveries",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_SubscriptionId",
                table: "tbl_PushDeliveries",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_UserId",
                table: "tbl_PushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_DateTimeCreated",
                table: "tbl_PushSubscriptions",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_DateTimeModified",
                table: "tbl_PushSubscriptions",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_IsDeleted",
                table: "tbl_PushSubscriptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_LastModifiedBy",
                table: "tbl_PushSubscriptions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_PushSubscription_EndpointHash",
                table: "tbl_PushSubscriptions",
                column: "EndpointHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_BudgetLineItemId",
                table: "tbl_Receipts",
                column: "BudgetLineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_PaymentDate",
                table: "tbl_Receipts",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Receipts_CreatedById",
                table: "tbl_Receipts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Receipts_DateTimeCreated",
                table: "tbl_Receipts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Receipts_DateTimeModified",
                table: "tbl_Receipts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Receipts_IsDeleted",
                table: "tbl_Receipts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Receipts_LastModifiedBy",
                table: "tbl_Receipts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RoleValues_DateTimeCreated",
                table: "tbl_RoleValues",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RoleValues_DateTimeModified",
                table: "tbl_RoleValues",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RoleValues_IsDeleted",
                table: "tbl_RoleValues",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RoleValues_LastModifiedBy",
                table: "tbl_RoleValues",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StageClaim_ProjectId",
                table: "tbl_StageClaims",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StageClaim_StageId",
                table: "tbl_StageClaims",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_ClaimedById",
                table: "tbl_StageClaims",
                column: "ClaimedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_ClearedById",
                table: "tbl_StageClaims",
                column: "ClearedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_DateTimeCreated",
                table: "tbl_StageClaims",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_DateTimeModified",
                table: "tbl_StageClaims",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_IsDeleted",
                table: "tbl_StageClaims",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_LastModifiedBy",
                table: "tbl_StageClaims",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Stage_ProjectId",
                table: "tbl_Stages",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_DateTimeCreated",
                table: "tbl_Stages",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_DateTimeModified",
                table: "tbl_Stages",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_IsDeleted",
                table: "tbl_Stages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_LastModifiedBy",
                table: "tbl_Stages",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_ParentStageId",
                table: "tbl_Stages",
                column: "ParentStageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_DateTimeCreated",
                table: "tbl_SubscriptionRequests",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_DateTimeModified",
                table: "tbl_SubscriptionRequests",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_IsDeleted",
                table: "tbl_SubscriptionRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_LastModifiedBy",
                table: "tbl_SubscriptionRequests",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_DateTimeCreated",
                table: "tbl_SubscriptionSeats",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_DateTimeModified",
                table: "tbl_SubscriptionSeats",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_IsDeleted",
                table: "tbl_SubscriptionSeats",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_LastModifiedBy",
                table: "tbl_SubscriptionSeats",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_RequestId_Email",
                table: "tbl_SubscriptionSeats",
                columns: new[] { "RequestId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SyncLogs_DateTimeCreated",
                table: "tbl_SyncLogs",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SyncLogs_DateTimeModified",
                table: "tbl_SyncLogs",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SyncLogs_IsDeleted",
                table: "tbl_SyncLogs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SyncLogs_LastModifiedBy",
                table: "tbl_SyncLogs",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_DateTimeCreated",
                table: "tbl_TenantMemberships",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_DateTimeModified",
                table: "tbl_TenantMemberships",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_IsDeleted",
                table: "tbl_TenantMemberships",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_LastModifiedBy",
                table: "tbl_TenantMemberships",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMembership_UserId",
                table: "tbl_TenantMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_TenantMembership_User_Tenant",
                table: "tbl_TenantMemberships",
                columns: new[] { "UserId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_ApprovedById",
                table: "tbl_Variations",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_CommitmentId",
                table: "tbl_Variations",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_DateTimeCreated",
                table: "tbl_Variations",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_DateTimeModified",
                table: "tbl_Variations",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_IsDeleted",
                table: "tbl_Variations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_LastModifiedBy",
                table: "tbl_Variations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_RaisedById",
                table: "tbl_Variations",
                column: "RaisedById");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProjectId",
                table: "tbl_Variations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_StageId",
                table: "tbl_Variations",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_DateTimeCreated",
                table: "tbl_WorksReports",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_DateTimeModified",
                table: "tbl_WorksReports",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_IsDeleted",
                table: "tbl_WorksReports",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_IssuedById",
                table: "tbl_WorksReports",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_LastModifiedBy",
                table: "tbl_WorksReports",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorksReport_Project_AsAt",
                table: "tbl_WorksReports",
                columns: new[] { "ProjectId", "AsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorksReport_Project_Trigger",
                table: "tbl_WorksReports",
                columns: new[] { "ProjectId", "TriggerKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "tbl_Annotations");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactPosters");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactRefs");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactRevisions");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactTexts");

            migrationBuilder.DropTable(
                name: "tbl_BriefPublications");

            migrationBuilder.DropTable(
                name: "tbl_ClaimEvidence");

            migrationBuilder.DropTable(
                name: "tbl_CommitmentEstimates");

            migrationBuilder.DropTable(
                name: "tbl_CommitmentLinks");

            migrationBuilder.DropTable(
                name: "tbl_Configuration");

            migrationBuilder.DropTable(
                name: "tbl_EmployeeApprovals");

            migrationBuilder.DropTable(
                name: "tbl_ExtractionProposals");

            migrationBuilder.DropTable(
                name: "tbl_ExtractionRuns");

            migrationBuilder.DropTable(
                name: "tbl_Flags");

            migrationBuilder.DropTable(
                name: "tbl_FundingEntries");

            migrationBuilder.DropTable(
                name: "tbl_Logs");

            migrationBuilder.DropTable(
                name: "tbl_MediaBindings");

            migrationBuilder.DropTable(
                name: "tbl_ProgressComments");

            migrationBuilder.DropTable(
                name: "tbl_ProjectPreferences");

            migrationBuilder.DropTable(
                name: "tbl_ProjectSubscriptions");

            migrationBuilder.DropTable(
                name: "tbl_PushDeliveries");

            migrationBuilder.DropTable(
                name: "tbl_Receipts");

            migrationBuilder.DropTable(
                name: "tbl_RoleValues");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionSeats");

            migrationBuilder.DropTable(
                name: "tbl_SyncLogs");

            migrationBuilder.DropTable(
                name: "tbl_TenantMemberships");

            migrationBuilder.DropTable(
                name: "tbl_Variations");

            migrationBuilder.DropTable(
                name: "tbl_WorksReports");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "tbl_Documents");

            migrationBuilder.DropTable(
                name: "tbl_ProgressReadings");

            migrationBuilder.DropTable(
                name: "tbl_StageClaims");

            migrationBuilder.DropTable(
                name: "tbl_Tenants");

            migrationBuilder.DropTable(
                name: "tbl_IngestedMessages");

            migrationBuilder.DropTable(
                name: "tbl_ProgressImages");

            migrationBuilder.DropTable(
                name: "tbl_PushSubscriptions");

            migrationBuilder.DropTable(
                name: "tbl_BudgetLineItems");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionRequests");

            migrationBuilder.DropTable(
                name: "tbl_Commitments");

            migrationBuilder.DropTable(
                name: "tbl_IngestBatches");

            migrationBuilder.DropTable(
                name: "tbl_ProgressUpdates");

            migrationBuilder.DropTable(
                name: "tbl_ProjectMembers");

            migrationBuilder.DropTable(
                name: "tbl_Artifacts");

            migrationBuilder.DropTable(
                name: "tbl_Deliverables");

            migrationBuilder.DropTable(
                name: "tbl_Stages");

            migrationBuilder.DropTable(
                name: "tbl_Projects_RS");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
