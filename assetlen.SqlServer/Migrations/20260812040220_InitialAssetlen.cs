using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialAssetlen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aboutme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true),
                    IsEmployee = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Logs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShiftId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogTypeId = table.Column<int>(type: "int", nullable: true),
                    OldQty = table.Column<int>(type: "int", nullable: true),
                    NewQty = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_RoleValues",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    RoleValue = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RoleValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SubscriptionRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RequestedSeats = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SubmittedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubmittedByEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuotedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuoteCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuoteNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuotedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuotedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    QuotedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SubscriptionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SyncLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserJwt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false, defaultValueSql: "NEWID()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CocurrencyKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    keyHarsh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastRenewal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BussinessRegNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfEmployees = table.Column<int>(type: "int", nullable: true),
                    AnnualRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CEO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: true),
                    StockSymbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "VerificationCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    ResendCount = table.Column<int>(type: "int", nullable: false),
                    LastResentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrowserType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceFingerprint = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoginCount = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApproverUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ExpectedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvestorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProjectManagerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsFirstFreeProject = table.Column<bool>(type: "bit", nullable: false),
                    IsSubscriptionActive = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ParentProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_SubscriptionSeats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LinkedUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettingID = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Specialization = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_ProjectSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    InvestorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "tbl_BudgetLineItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_FundingEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConfirmedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_ProgressUpdates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    HasIssues = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_Receipts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BudgetLineItemId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceiptImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                name: "tbl_ProgressImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressImages_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Flags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProgressUpdateId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProgressImageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssignedToId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolvedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNudgeAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsNudgeArchived = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProgressImageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ParentCommentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
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
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

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
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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
                name: "IX_Flag_AssignedToId",
                table: "tbl_Flags",
                column: "AssignedToId");

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
                name: "IX_tbl_ProgressImages_DateTimeCreated",
                table: "tbl_ProgressImages",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_DateTimeModified",
                table: "tbl_ProgressImages",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_IsDeleted",
                table: "tbl_ProgressImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_LastModifiedBy",
                table: "tbl_ProgressImages",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_CreatedAt",
                table: "tbl_ProgressUpdates",
                column: "DateTimeCreated");

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
                name: "IX_Project_InvestorId",
                table: "tbl_Projects_RS",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ParentProjectId",
                table: "tbl_Projects_RS",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectManagerId",
                table: "tbl_Projects_RS",
                column: "ProjectManagerId");

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
                name: "tbl_Configuration");

            migrationBuilder.DropTable(
                name: "tbl_EmployeeApprovals");

            migrationBuilder.DropTable(
                name: "tbl_Flags");

            migrationBuilder.DropTable(
                name: "tbl_FundingEntries");

            migrationBuilder.DropTable(
                name: "tbl_Logs");

            migrationBuilder.DropTable(
                name: "tbl_ProgressComments");

            migrationBuilder.DropTable(
                name: "tbl_ProjectMembers");

            migrationBuilder.DropTable(
                name: "tbl_ProjectSubscriptions");

            migrationBuilder.DropTable(
                name: "tbl_Receipts");

            migrationBuilder.DropTable(
                name: "tbl_RoleValues");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionSeats");

            migrationBuilder.DropTable(
                name: "tbl_SyncLogs");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "tbl_Tenants");

            migrationBuilder.DropTable(
                name: "tbl_ProgressImages");

            migrationBuilder.DropTable(
                name: "tbl_BudgetLineItems");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionRequests");

            migrationBuilder.DropTable(
                name: "tbl_ProgressUpdates");

            migrationBuilder.DropTable(
                name: "tbl_Stages");

            migrationBuilder.DropTable(
                name: "tbl_Projects_RS");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
