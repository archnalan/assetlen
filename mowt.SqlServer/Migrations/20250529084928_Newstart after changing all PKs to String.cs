using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class NewstartafterchangingallPKstoString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lastname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aboutme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "tbl_CashItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CashItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CashItems_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_category",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    category = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    hideInPOS = table.Column<bool>(type: "bit", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_category_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Configuration",
                columns: table => new
                {
                    SettingID = table.Column<int>(type: "int", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Configuration", x => x.SettingID);
                    table.ForeignKey(
                        name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_customerDeposit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    drawerID = table.Column<int>(type: "int", nullable: true),
                    customerID = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    comment = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    dateTimeDeposited = table.Column<DateTime>(type: "datetime", nullable: true),
                    change = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    userID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerDeposit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_customerDeposit_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_customerPricing",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    customerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priceGroupID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priceInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    priceExc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    taxID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sortOrder = table.Column<int>(type: "int", nullable: true),
                    costInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    costExc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerPricing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_customerPricing_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Contact = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    creditLimit = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    Company = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Customers_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_discounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DiscountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    discountValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    isValuePercentage = table.Column<bool>(type: "bit", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_discounts_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExpenseType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExpenseType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ExpenseType_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_location",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Location = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_location_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
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
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    SaleId = table.Column<int>(type: "int", nullable: true),
                    LogTypeId = table.Column<int>(type: "int", nullable: true),
                    OldQty = table.Column<int>(type: "int", nullable: true),
                    NewQty = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Logs_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_OrderProcesses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<byte[]>(type: "varbinary(150)", maxLength: 150, nullable: true),
                    SortID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OrderProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_OrderProcesses_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_OrderStatus",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OrderName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    sortOrder = table.Column<int>(type: "int", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OrderStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_OrderStatus_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_paymentAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    paymentTypeID = table.Column<int>(type: "int", nullable: false),
                    paymentAccountName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    openingBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_paymentAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_paymentAccounts_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_paymentMode",
                columns: table => new
                {
                    PaymentModeID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_payments", x => x.PaymentModeID);
                    table.ForeignKey(
                        name: "FK_tbl_paymentMode_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductReceiving",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime", nullable: true),
                    GRNSupplierNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ReceivedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceChanged = table.Column<bool>(type: "bit", nullable: true),
                    NewCostInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    NewPriceInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PriceChangeScheduled = table.Column<DateTime>(type: "datetime", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    creditSupplierAcc = table.Column<bool>(type: "bit", nullable: true),
                    supplierAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costExc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    costInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductReceiving", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductReceiving_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductRelationships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    hasAsubProductID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isAsubProductID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qty = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    sortOrder = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductRelationships_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Refunds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    saleID = table.Column<int>(type: "int", nullable: true),
                    refundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    refundDateTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    refundedBy = table.Column<int>(type: "int", nullable: true),
                    refundComment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    shiftID = table.Column<int>(type: "int", nullable: true),
                    toCustomerID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Refunds_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
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
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_RoleValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_RoleValues_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_segment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    segment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    hideInPOS = table.Column<bool>(type: "bit", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_segment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_segment_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Sizes",
                columns: table => new
                {
                    SizeID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Sizes", x => x.SizeID);
                    table.ForeignKey(
                        name: "FK_tbl_Sizes_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SlipLayout",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<int>(type: "int", nullable: false),
                    IsBold = table.Column<bool>(type: "bit", nullable: false),
                    IsItalic = table.Column<bool>(type: "bit", nullable: false),
                    IsUnderline = table.Column<bool>(type: "bit", nullable: false),
                    Alignment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alignment2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alignment3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alignment4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alignment5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontFamily = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RectWidth = table.Column<int>(type: "int", nullable: false),
                    SlipID = table.Column<int>(type: "int", nullable: false),
                    PrintItemType = table.Column<int>(type: "int", nullable: false),
                    isMultiLine = table.Column<bool>(type: "bit", nullable: false),
                    LineHeight = table.Column<double>(type: "float", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SlipLayout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SlipLayout_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Supplier",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    Contact = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    creditLimit = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    Company = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Supplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Supplier_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SupplierPayment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    userID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    dateTimePayed = table.Column<DateTime>(type: "datetime", nullable: true),
                    paymentID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SupplierPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SupplierPayment_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
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
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SyncLogs_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_tax",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    taxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    taxDescription = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_tax", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_tax_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_UniqueFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UniqueField = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UniqueFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UniqueFields_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    userName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    passwordHarsh = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    passwordSalt = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    fullName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    tel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    cardNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    userType = table.Column<int>(type: "int", nullable: true),
                    profilePic = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    deleted = table.Column<int>(type: "int", nullable: true),
                    col12 = table.Column<bool>(type: "bit", nullable: true),
                    col13 = table.Column<bool>(type: "bit", nullable: true),
                    col15 = table.Column<bool>(type: "bit", nullable: true),
                    col1 = table.Column<bool>(type: "bit", nullable: true),
                    col3 = table.Column<bool>(type: "bit", nullable: true),
                    col5 = table.Column<bool>(type: "bit", nullable: true),
                    col2 = table.Column<bool>(type: "bit", nullable: true),
                    col6 = table.Column<bool>(type: "bit", nullable: true),
                    col7 = table.Column<bool>(type: "bit", nullable: true),
                    col8 = table.Column<bool>(type: "bit", nullable: true),
                    col9 = table.Column<bool>(type: "bit", nullable: true),
                    col11 = table.Column<bool>(type: "bit", nullable: true),
                    col10 = table.Column<bool>(type: "bit", nullable: true),
                    col19 = table.Column<bool>(type: "bit", nullable: true),
                    col18 = table.Column<bool>(type: "bit", nullable: true),
                    col17 = table.Column<bool>(type: "bit", nullable: true),
                    col16 = table.Column<bool>(type: "bit", nullable: true),
                    col4 = table.Column<bool>(type: "bit", nullable: true),
                    col14 = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    col20 = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_users_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_shifts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    userId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    dateTimeOpened = table.Column<DateTime>(type: "datetime", nullable: true),
                    openingBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    currentBalance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    dateTimeClosed = table.Column<DateTime>(type: "datetime", nullable: true),
                    activeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subActiveId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    shiftEndCash = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndCard = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndCheque = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    comment = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    drawerStatus = table.Column<bool>(type: "bit", nullable: true),
                    shiftEndBank = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndAcc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_shifts_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_shifts_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_transaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    transactionDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    soldBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    saleTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    change = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    shiftId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customerId = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    transactionStatus = table.Column<int>(type: "int", nullable: true),
                    saleAgentID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    quotationID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    orderStatus = table.Column<int>(type: "int", nullable: true),
                    ImportedId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transactionComment = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_transaction_Users_saleAgentID",
                        column: x => x.saleAgentID,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transaction_Users_soldBy",
                        column: x => x.soldBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transaction_tbl_Customers_customerId",
                        column: x => x.customerId,
                        principalTable: "tbl_Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transaction_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PaymentModeID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    saleID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CustomerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardRef = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ChequeNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    NameOnCheque = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    BankID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CustomerDepositID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    SupplierID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SupplierPaymentID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    Change = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpenseID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Payments_1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Payments_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_SupplierPayment_SupplierPaymentID",
                        column: x => x.SupplierPaymentID,
                        principalTable: "tbl_SupplierPayment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "tbl_Supplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_customerDeposit_CustomerDepositID",
                        column: x => x.CustomerDepositID,
                        principalTable: "tbl_customerDeposit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_paymentMode_PaymentModeID",
                        column: x => x.PaymentModeID,
                        principalTable: "tbl_paymentMode",
                        principalColumn: "PaymentModeID");
                });

            migrationBuilder.CreateTable(
                name: "tbl_products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    productCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    barCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    productName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    costExclusive = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    costInclusive = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    inStock = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    priceExclusive = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    priceExclusive2 = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    priceInclusive = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    priceInclusive2 = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    categoryId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    location = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    segmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    supplierId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productImage = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    createdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    createdBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    trackInventory = table.Column<bool>(type: "bit", nullable: true),
                    ReOrderLevel = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReOrderQty = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Favourite = table.Column<bool>(type: "bit", nullable: true),
                    hasSubProduct = table.Column<bool>(type: "bit", nullable: true),
                    isAsubProduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    compoundCostPricing = table.Column<int>(type: "int", nullable: true),
                    tax = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    costIncStatus = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_products_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_products_tbl_tax_tax",
                        column: x => x.tax,
                        principalTable: "tbl_tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Expense",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CustomerID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    SupplierID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    EmployeeID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExpenseType = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Comment = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    shiftID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    dateTimePayed = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Expense_Users_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "tbl_Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_ExpenseType_ExpenseType",
                        column: x => x.ExpenseType,
                        principalTable: "tbl_ExpenseType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "tbl_Supplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_shifts_shiftID",
                        column: x => x.shiftID,
                        principalTable: "tbl_shifts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ShiftClosureSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShiftId = table.Column<string>(type: "nvarchar(40)", nullable: false),
                    PaymentModeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaleTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalCounted = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ShiftExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalExpected = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalShortage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ShiftClosureSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ShiftClosureSummaries_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_ShiftClosureSummaries_tbl_shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "tbl_shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_transactionDetail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    productID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    qty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    costExc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    costInc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    priceInc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    priceExc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    taxID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    taxPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    discountID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    discountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    transactionID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    totalPriceInc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    totalPriceExc = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    sortOrder = table.Column<int>(type: "int", nullable: true),
                    costIncState = table.Column<bool>(type: "bit", nullable: true),
                    specialPricingUsed = table.Column<bool>(type: "bit", nullable: true),
                    ImportedId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tbl_TransactionId = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transactionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_discounts_discountID",
                        column: x => x.discountID,
                        principalTable: "tbl_discounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_products_productID",
                        column: x => x.productID,
                        principalTable: "tbl_products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_tax_taxID",
                        column: x => x.taxID,
                        principalTable: "tbl_tax",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_transaction_tbl_TransactionId",
                        column: x => x.tbl_TransactionId,
                        principalTable: "tbl_transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_Amount",
                table: "tbl_CashItems",
                column: "Amount",
                unique: true,
                filter: "[Amount] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_DateTimeCreated",
                table: "tbl_CashItems",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_DateTimeModified",
                table: "tbl_CashItems",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_IsDeleted",
                table: "tbl_CashItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_LastModifiedBy",
                table: "tbl_CashItems",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_TenantId",
                table: "tbl_CashItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_DateTimeCreated",
                table: "tbl_category",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_DateTimeModified",
                table: "tbl_category",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_IsDeleted",
                table: "tbl_category",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_LastModifiedBy",
                table: "tbl_category",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_TenantId",
                table: "tbl_category",
                column: "TenantId");

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
                name: "IX_tbl_customerDeposit_customerID",
                table: "tbl_customerDeposit",
                column: "customerID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_DateTimeCreated",
                table: "tbl_customerDeposit",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_dateTimeDeposited",
                table: "tbl_customerDeposit",
                column: "dateTimeDeposited");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_dateTimeDeposited_customerID",
                table: "tbl_customerDeposit",
                columns: new[] { "dateTimeDeposited", "customerID" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_DateTimeModified",
                table: "tbl_customerDeposit",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_IsDeleted",
                table: "tbl_customerDeposit",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_LastModifiedBy",
                table: "tbl_customerDeposit",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_TenantId",
                table: "tbl_customerDeposit",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_DateTimeCreated",
                table: "tbl_customerPricing",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_DateTimeModified",
                table: "tbl_customerPricing",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_isDeleted",
                table: "tbl_customerPricing",
                column: "isDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_LastModifiedBy",
                table: "tbl_customerPricing",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_TenantId",
                table: "tbl_customerPricing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_DateTimeCreated",
                table: "tbl_Customers",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_DateTimeModified",
                table: "tbl_Customers",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_IsDeleted",
                table: "tbl_Customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_LastModifiedBy",
                table: "tbl_Customers",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_TenantId",
                table: "tbl_Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_DateTimeCreated",
                table: "tbl_discounts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_DateTimeModified",
                table: "tbl_discounts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_IsDeleted",
                table: "tbl_discounts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_LastModifiedBy",
                table: "tbl_discounts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_TenantId",
                table: "tbl_discounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_CustomerID",
                table: "tbl_Expense",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_DateTimeCreated",
                table: "tbl_Expense",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_DateTimeModified",
                table: "tbl_Expense",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_EmployeeID",
                table: "tbl_Expense",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_ExpenseType",
                table: "tbl_Expense",
                column: "ExpenseType");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_IsDeleted",
                table: "tbl_Expense",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_LastModifiedBy",
                table: "tbl_Expense",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_shiftID",
                table: "tbl_Expense",
                column: "shiftID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_SupplierID",
                table: "tbl_Expense",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_TenantId",
                table: "tbl_Expense",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_DateTimeCreated",
                table: "tbl_ExpenseType",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_DateTimeModified",
                table: "tbl_ExpenseType",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_IsDeleted",
                table: "tbl_ExpenseType",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_LastModifiedBy",
                table: "tbl_ExpenseType",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_TenantId",
                table: "tbl_ExpenseType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_DateTimeCreated",
                table: "tbl_location",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_DateTimeModified",
                table: "tbl_location",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_isDeleted",
                table: "tbl_location",
                column: "isDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_LastModifiedBy",
                table: "tbl_location",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_TenantId",
                table: "tbl_location",
                column: "TenantId");

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
                name: "IX_tbl_Logs_TenantId",
                table: "tbl_Logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_DateTimeCreated",
                table: "tbl_OrderProcesses",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_DateTimeModified",
                table: "tbl_OrderProcesses",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_IsDeleted",
                table: "tbl_OrderProcesses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_LastModifiedBy",
                table: "tbl_OrderProcesses",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_TenantId",
                table: "tbl_OrderProcesses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_DateTimeCreated",
                table: "tbl_OrderStatus",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_DateTimeModified",
                table: "tbl_OrderStatus",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_isDeleted",
                table: "tbl_OrderStatus",
                column: "isDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_LastModifiedBy",
                table: "tbl_OrderStatus",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_TenantId",
                table: "tbl_OrderStatus",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_DateTimeCreated",
                table: "tbl_paymentAccounts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_DateTimeModified",
                table: "tbl_paymentAccounts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_IsDeleted",
                table: "tbl_paymentAccounts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_LastModifiedBy",
                table: "tbl_paymentAccounts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_TenantId",
                table: "tbl_paymentAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_DateTimeCreated",
                table: "tbl_paymentMode",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_DateTimeModified",
                table: "tbl_paymentMode",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_IsDeleted",
                table: "tbl_paymentMode",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_LastModifiedBy",
                table: "tbl_paymentMode",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_PaymentModeID",
                table: "tbl_paymentMode",
                column: "PaymentModeID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_TenantId",
                table: "tbl_paymentMode",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_CustomerDepositID",
                table: "tbl_Payments",
                column: "CustomerDepositID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_DateTimeCreated",
                table: "tbl_Payments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_DateTimeModified",
                table: "tbl_Payments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_EmployeeId",
                table: "tbl_Payments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_IsDeleted",
                table: "tbl_Payments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_LastModifiedBy",
                table: "tbl_Payments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_PaymentModeID",
                table: "tbl_Payments",
                column: "PaymentModeID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_purchaseID_PaymentModeID",
                table: "tbl_Payments",
                columns: new[] { "saleID", "PaymentModeID" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_saleID",
                table: "tbl_Payments",
                column: "saleID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_SupplierID",
                table: "tbl_Payments",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_SupplierPaymentID",
                table: "tbl_Payments",
                column: "SupplierPaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_TenantId",
                table: "tbl_Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_DateTimeCreated",
                table: "tbl_ProductReceiving",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_DateTimeModified",
                table: "tbl_ProductReceiving",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_IsDeleted",
                table: "tbl_ProductReceiving",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_LastModifiedBy",
                table: "tbl_ProductReceiving",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_TenantId",
                table: "tbl_ProductReceiving",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ__Received__DE143AD31592E241",
                table: "tbl_ProductReceiving",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_DateTimeCreated",
                table: "tbl_ProductRelationships",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_DateTimeModified",
                table: "tbl_ProductRelationships",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_IsDeleted",
                table: "tbl_ProductRelationships",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_LastModifiedBy",
                table: "tbl_ProductRelationships",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_TenantId",
                table: "tbl_ProductRelationships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_categoryId",
                table: "tbl_products",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_categoryId_segmentId",
                table: "tbl_products",
                columns: new[] { "categoryId", "segmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_DateTimeCreated",
                table: "tbl_products",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_DateTimeModified",
                table: "tbl_products",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_IsDeleted",
                table: "tbl_products",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_LastModifiedBy",
                table: "tbl_products",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_segmentId",
                table: "tbl_products",
                column: "segmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_tax",
                table: "tbl_products",
                column: "tax");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_TenantId",
                table: "tbl_products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_DateTimeCreated",
                table: "tbl_Refunds",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_DateTimeModified",
                table: "tbl_Refunds",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_IsDeleted",
                table: "tbl_Refunds",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_LastModifiedBy",
                table: "tbl_Refunds",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_refundDateTime",
                table: "tbl_Refunds",
                column: "refundDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_refundDateTime_toCustomerID",
                table: "tbl_Refunds",
                columns: new[] { "refundDateTime", "toCustomerID" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_TenantId",
                table: "tbl_Refunds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_toCustomerID",
                table: "tbl_Refunds",
                column: "toCustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_tSaleID",
                table: "tbl_Refunds",
                column: "saleID");

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
                name: "IX_tbl_RoleValues_TenantId",
                table: "tbl_RoleValues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_DateTimeCreated",
                table: "tbl_segment",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_DateTimeModified",
                table: "tbl_segment",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_isDeleted",
                table: "tbl_segment",
                column: "isDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_LastModifiedBy",
                table: "tbl_segment",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_TenantId",
                table: "tbl_segment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_DateTimeCreated",
                table: "tbl_ShiftClosureSummaries",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_DateTimeModified",
                table: "tbl_ShiftClosureSummaries",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_IsDeleted",
                table: "tbl_ShiftClosureSummaries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_LastModifiedBy",
                table: "tbl_ShiftClosureSummaries",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_ShiftId",
                table: "tbl_ShiftClosureSummaries",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_TenantId",
                table: "tbl_ShiftClosureSummaries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_DateTimeCreated",
                table: "tbl_shifts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_DateTimeModified",
                table: "tbl_shifts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_IsDeleted",
                table: "tbl_shifts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_LastModifiedBy",
                table: "tbl_shifts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_TenantId",
                table: "tbl_shifts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_userId",
                table: "tbl_shifts",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_DateTimeCreated",
                table: "tbl_Sizes",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_DateTimeModified",
                table: "tbl_Sizes",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_IsDeleted",
                table: "tbl_Sizes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_LastModifiedBy",
                table: "tbl_Sizes",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_TenantId",
                table: "tbl_Sizes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_DateTimeCreated",
                table: "tbl_SlipLayout",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_DateTimeModified",
                table: "tbl_SlipLayout",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_IsDeleted",
                table: "tbl_SlipLayout",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_LastModifiedBy",
                table: "tbl_SlipLayout",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_TenantId",
                table: "tbl_SlipLayout",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_DateTimeCreated",
                table: "tbl_Supplier",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_DateTimeModified",
                table: "tbl_Supplier",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_IsDeleted",
                table: "tbl_Supplier",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_LastModifiedBy",
                table: "tbl_Supplier",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_TenantId",
                table: "tbl_Supplier",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_DateTimeCreated",
                table: "tbl_SupplierPayment",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_DateTimeModified",
                table: "tbl_SupplierPayment",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_dateTimePayed",
                table: "tbl_SupplierPayment",
                column: "dateTimePayed");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_dateTimePayed_supplierID",
                table: "tbl_SupplierPayment",
                columns: new[] { "dateTimePayed", "SupplierID" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_IsDeleted",
                table: "tbl_SupplierPayment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_LastModifiedBy",
                table: "tbl_SupplierPayment",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_supplierID",
                table: "tbl_SupplierPayment",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_TenantId",
                table: "tbl_SupplierPayment",
                column: "TenantId");

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
                name: "IX_tbl_SyncLogs_TenantId",
                table: "tbl_SyncLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_DateTimeCreated",
                table: "tbl_tax",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_DateTimeModified",
                table: "tbl_tax",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_IsDeleted",
                table: "tbl_tax",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_LastModifiedBy",
                table: "tbl_tax",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_TenantId",
                table: "tbl_tax",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_customerId",
                table: "tbl_transaction",
                column: "customerId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_DateTimeCreated",
                table: "tbl_transaction",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_DateTimeModified",
                table: "tbl_transaction",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_IsDeleted",
                table: "tbl_transaction",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_LastModifiedBy",
                table: "tbl_transaction",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_soldBy",
                table: "tbl_transaction",
                column: "soldBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_supplierId",
                table: "tbl_transaction",
                column: "saleAgentID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_TenantId",
                table: "tbl_transaction",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_transactionDate",
                table: "tbl_transaction",
                column: "transactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_transactionDate_customerId",
                table: "tbl_transaction",
                columns: new[] { "transactionDate", "customerId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_transactionDate_SaleAgentId",
                table: "tbl_transaction",
                columns: new[] { "transactionDate", "saleAgentID" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_transactionDate_transactionStatus",
                table: "tbl_transaction",
                columns: new[] { "transactionDate", "transactionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_transactionStatus",
                table: "tbl_transaction",
                column: "transactionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_DateTimeCreated",
                table: "tbl_transactionDetail",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_DateTimeModified",
                table: "tbl_transactionDetail",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_discountID",
                table: "tbl_transactionDetail",
                column: "discountID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_IsDeleted",
                table: "tbl_transactionDetail",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_LastModifiedBy",
                table: "tbl_transactionDetail",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_productID",
                table: "tbl_transactionDetail",
                column: "productID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_taxID",
                table: "tbl_transactionDetail",
                column: "taxID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_tbl_TransactionId",
                table: "tbl_transactionDetail",
                column: "tbl_TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_TenantId",
                table: "tbl_transactionDetail",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_transactionID",
                table: "tbl_transactionDetail",
                column: "transactionID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_DateTimeCreated",
                table: "tbl_UniqueFields",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_DateTimeModified",
                table: "tbl_UniqueFields",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_IsDeleted",
                table: "tbl_UniqueFields",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_LastModifiedBy",
                table: "tbl_UniqueFields",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_TenantId",
                table: "tbl_UniqueFields",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_DateTimeCreated",
                table: "tbl_users",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_DateTimeModified",
                table: "tbl_users",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_IsDeleted",
                table: "tbl_users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_LastModifiedBy",
                table: "tbl_users",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_TenantId",
                table: "tbl_users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DateTimeCreated",
                table: "Users",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DateTimeModified",
                table: "Users",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastModifiedBy",
                table: "Users",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "tbl_CashItems");

            migrationBuilder.DropTable(
                name: "tbl_category");

            migrationBuilder.DropTable(
                name: "tbl_Configuration");

            migrationBuilder.DropTable(
                name: "tbl_customerPricing");

            migrationBuilder.DropTable(
                name: "tbl_Expense");

            migrationBuilder.DropTable(
                name: "tbl_location");

            migrationBuilder.DropTable(
                name: "tbl_Logs");

            migrationBuilder.DropTable(
                name: "tbl_OrderProcesses");

            migrationBuilder.DropTable(
                name: "tbl_OrderStatus");

            migrationBuilder.DropTable(
                name: "tbl_paymentAccounts");

            migrationBuilder.DropTable(
                name: "tbl_Payments");

            migrationBuilder.DropTable(
                name: "tbl_ProductReceiving");

            migrationBuilder.DropTable(
                name: "tbl_ProductRelationships");

            migrationBuilder.DropTable(
                name: "tbl_Refunds");

            migrationBuilder.DropTable(
                name: "tbl_RoleValues");

            migrationBuilder.DropTable(
                name: "tbl_segment");

            migrationBuilder.DropTable(
                name: "tbl_ShiftClosureSummaries");

            migrationBuilder.DropTable(
                name: "tbl_Sizes");

            migrationBuilder.DropTable(
                name: "tbl_SlipLayout");

            migrationBuilder.DropTable(
                name: "tbl_SyncLogs");

            migrationBuilder.DropTable(
                name: "tbl_transactionDetail");

            migrationBuilder.DropTable(
                name: "tbl_UniqueFields");

            migrationBuilder.DropTable(
                name: "tbl_users");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "tbl_ExpenseType");

            migrationBuilder.DropTable(
                name: "tbl_SupplierPayment");

            migrationBuilder.DropTable(
                name: "tbl_Supplier");

            migrationBuilder.DropTable(
                name: "tbl_customerDeposit");

            migrationBuilder.DropTable(
                name: "tbl_paymentMode");

            migrationBuilder.DropTable(
                name: "tbl_shifts");

            migrationBuilder.DropTable(
                name: "tbl_discounts");

            migrationBuilder.DropTable(
                name: "tbl_products");

            migrationBuilder.DropTable(
                name: "tbl_transaction");

            migrationBuilder.DropTable(
                name: "tbl_tax");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "tbl_Customers");

            migrationBuilder.DropTable(
                name: "tbl_Tenants");
        }
    }
}
