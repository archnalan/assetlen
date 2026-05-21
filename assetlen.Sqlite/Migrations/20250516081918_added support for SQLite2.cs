using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class addedsupportforSQLite2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Tenants",
                columns: table => new
                {
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    BussinessRegNumber = table.Column<string>(type: "TEXT", nullable: true),
                    TaxIdentificationNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Industry = table.Column<string>(type: "TEXT", nullable: true),
                    NumberOfEmployees = table.Column<int>(type: "INTEGER", nullable: true),
                    AnnualRevenue = table.Column<string>(type: "TEXT", nullable: true),
                    CEO = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: true),
                    StockSymbol = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    Lastname = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Aboutme = table.Column<string>(type: "TEXT", nullable: true),
                    Contacts = table.Column<string>(type: "TEXT", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CoverPhotoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "tbl_CashItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CashItems", x => x.id);
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
                    categoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    hideInPOS = table.Column<bool>(type: "INTEGER", nullable: true),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_category", x => x.categoryId);
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
                    SettingID = table.Column<int>(type: "INTEGER", nullable: false),
                    StringValue = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                    depositID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    drawerID = table.Column<int>(type: "INTEGER", nullable: true),
                    customerID = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<string>(type: "TEXT", nullable: true),
                    comment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    dateTimeDeposited = table.Column<DateTime>(type: "TEXT", nullable: true),
                    change = table.Column<string>(type: "TEXT", nullable: true),
                    userID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerDeposit", x => x.depositID);
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
                    CustomerPricingID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    customerID = table.Column<int>(type: "INTEGER", nullable: true),
                    productID = table.Column<int>(type: "INTEGER", nullable: true),
                    priceGroupID = table.Column<int>(type: "INTEGER", nullable: true),
                    priceInc = table.Column<string>(type: "TEXT", nullable: true),
                    priceExc = table.Column<string>(type: "TEXT", nullable: true),
                    taxID = table.Column<int>(type: "INTEGER", nullable: true),
                    sortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    costInc = table.Column<string>(type: "TEXT", nullable: true),
                    costExc = table.Column<string>(type: "TEXT", nullable: true),
                    isDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerPricing", x => x.CustomerPricingID);
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
                    CustomerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    Contact = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    creditLimit = table.Column<string>(type: "TEXT", nullable: true),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    Company = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Customers", x => x.CustomerID);
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
                    discountID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscountName = table.Column<string>(type: "TEXT", nullable: true),
                    discountValue = table.Column<string>(type: "TEXT", nullable: true),
                    isValuePercentage = table.Column<bool>(type: "INTEGER", nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_discounts", x => x.discountID);
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
                    typeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExpenseType", x => x.typeID);
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
                    locationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Location = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    isDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_location", x => x.locationID);
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    MessageTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    Level = table.Column<string>(type: "TEXT", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Exception = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: true),
                    SaleId = table.Column<int>(type: "INTEGER", nullable: true),
                    LogTypeId = table.Column<int>(type: "INTEGER", nullable: true),
                    OldQty = table.Column<int>(type: "INTEGER", nullable: true),
                    NewQty = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<byte[]>(type: "BLOB", maxLength: 150, nullable: true),
                    SortID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    sortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    isDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OrderStatus", x => x.OrderID);
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
                    paymentAccountID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    paymentTypeID = table.Column<int>(type: "INTEGER", nullable: false),
                    paymentAccountName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    openingBalance = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_paymentAccounts", x => x.paymentAccountID);
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
                    PaymentModeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                    ReceiveProductID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductID = table.Column<int>(type: "INTEGER", nullable: true),
                    Qty = table.Column<string>(type: "TEXT", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GRNSupplierNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    ReceivedBy = table.Column<string>(type: "TEXT", nullable: true),
                    PriceChanged = table.Column<bool>(type: "INTEGER", nullable: true),
                    NewCostInc = table.Column<string>(type: "TEXT", nullable: true),
                    NewPriceInc = table.Column<string>(type: "TEXT", nullable: true),
                    PriceChangeScheduled = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OrderID = table.Column<int>(type: "INTEGER", nullable: true),
                    creditSupplierAcc = table.Column<bool>(type: "INTEGER", nullable: true),
                    supplierAccount = table.Column<int>(type: "INTEGER", nullable: true),
                    costExc = table.Column<string>(type: "TEXT", nullable: true),
                    costInc = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Received__DE143AD2C4AD0CE3", x => x.ReceiveProductID);
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
                    relationShipID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    hasAsubProductID = table.Column<int>(type: "INTEGER", nullable: true),
                    isAsubProductID = table.Column<int>(type: "INTEGER", nullable: true),
                    qty = table.Column<string>(type: "TEXT", nullable: true),
                    sortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductRelationships", x => x.relationShipID);
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
                    refundID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    saleID = table.Column<int>(type: "INTEGER", nullable: true),
                    refundAmount = table.Column<string>(type: "TEXT", nullable: true),
                    refundDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    refundedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    refundComment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    shiftID = table.Column<int>(type: "INTEGER", nullable: true),
                    toCustomerID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Table__B21984EF758995AB", x => x.refundID);
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
                    UserRoleID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: true),
                    RoleID = table.Column<int>(type: "INTEGER", nullable: true),
                    RoleValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserRoles", x => x.UserRoleID);
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
                    segmentid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    segment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    hideInPOS = table.Column<bool>(type: "INTEGER", nullable: true),
                    isDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSegment", x => x.segmentid);
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
                    SizeID = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: true),
                    Height = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                    PrintItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Text2 = table.Column<string>(type: "TEXT", nullable: false),
                    Text3 = table.Column<string>(type: "TEXT", nullable: false),
                    Text4 = table.Column<string>(type: "TEXT", nullable: false),
                    Text5 = table.Column<string>(type: "TEXT", nullable: false),
                    FontSize = table.Column<int>(type: "INTEGER", nullable: false),
                    IsBold = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsItalic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsUnderline = table.Column<bool>(type: "INTEGER", nullable: false),
                    Alignment = table.Column<string>(type: "TEXT", nullable: false),
                    Alignment2 = table.Column<string>(type: "TEXT", nullable: false),
                    Alignment3 = table.Column<string>(type: "TEXT", nullable: false),
                    Alignment4 = table.Column<string>(type: "TEXT", nullable: false),
                    Alignment5 = table.Column<string>(type: "TEXT", nullable: false),
                    FontFamily = table.Column<string>(type: "TEXT", nullable: false),
                    RectWidth = table.Column<int>(type: "INTEGER", nullable: false),
                    SlipID = table.Column<int>(type: "INTEGER", nullable: false),
                    PrintItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    isMultiLine = table.Column<bool>(type: "INTEGER", nullable: false),
                    LineHeight = table.Column<double>(type: "REAL", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SlipLayout", x => x.PrintItemId);
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
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    Contact = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    VatNumber = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    creditLimit = table.Column<string>(type: "TEXT", nullable: true),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    Company = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Supplier", x => x.SupplierID);
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
                    SupplierPaymentID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userID = table.Column<int>(type: "INTEGER", nullable: true),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<string>(type: "TEXT", nullable: true),
                    dateTimePayed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    paymentID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SupplierPayment", x => x.SupplierPaymentID);
                    table.ForeignKey(
                        name: "FK_tbl_SupplierPayment_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_tax",
                columns: table => new
                {
                    taxID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    taxValue = table.Column<string>(type: "TEXT", nullable: true),
                    taxDescription = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_tax", x => x.taxID);
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
                    UniqueField = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UniqueFields", x => x.UniqueField);
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
                    userId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    passwordHarsh = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    passwordSalt = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    fullName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    tel = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    cardNo = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    userType = table.Column<int>(type: "INTEGER", nullable: true),
                    profilePic = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    deleted = table.Column<int>(type: "INTEGER", nullable: true),
                    col12 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col13 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col15 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col1 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col3 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col5 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col2 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col6 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col7 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col8 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col9 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col11 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col10 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col19 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col18 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col17 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col16 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col4 = table.Column<bool>(type: "INTEGER", nullable: true),
                    col14 = table.Column<bool>(type: "INTEGER", nullable: true, defaultValue: false),
                    col20 = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_users", x => x.userId);
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
                    shiftId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userId = table.Column<string>(type: "TEXT", nullable: true),
                    dateTimeOpened = table.Column<DateTime>(type: "TEXT", nullable: true),
                    openingBalance = table.Column<string>(type: "TEXT", nullable: true),
                    currentBalance = table.Column<string>(type: "TEXT", nullable: true),
                    shiftEndAmount = table.Column<string>(type: "TEXT", nullable: true),
                    dateTimeClosed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    activeId = table.Column<int>(type: "INTEGER", nullable: true),
                    subActiveId = table.Column<int>(type: "INTEGER", nullable: false),
                    shiftEndCash = table.Column<string>(type: "TEXT", nullable: true),
                    shiftEndCard = table.Column<string>(type: "TEXT", nullable: true),
                    shiftEndCheque = table.Column<string>(type: "TEXT", nullable: true),
                    comment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    drawerStatus = table.Column<bool>(type: "INTEGER", nullable: true),
                    shiftEndBank = table.Column<string>(type: "TEXT", nullable: true),
                    shiftEndAcc = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_shifts", x => x.shiftId);
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
                    transactionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    transactionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    soldBy = table.Column<string>(type: "TEXT", nullable: true),
                    saleTotal = table.Column<string>(type: "TEXT", nullable: true),
                    change = table.Column<string>(type: "TEXT", nullable: true),
                    shiftId = table.Column<int>(type: "INTEGER", nullable: true),
                    customerId = table.Column<int>(type: "INTEGER", nullable: true),
                    transactionStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    saleAgentID = table.Column<string>(type: "TEXT", nullable: true),
                    quotationID = table.Column<int>(type: "INTEGER", nullable: true),
                    orderStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    transactionComment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transaction", x => x.transactionId);
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
                        principalColumn: "CustomerID");
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
                    PaymentID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PaymentModeID = table.Column<int>(type: "INTEGER", nullable: true),
                    saleID = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<string>(type: "TEXT", nullable: true),
                    CustomerID = table.Column<int>(type: "INTEGER", nullable: true),
                    CardRef = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    ChequeNo = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    NameOnCheque = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    BankID = table.Column<int>(type: "INTEGER", nullable: true),
                    BankingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomerDepositID = table.Column<int>(type: "INTEGER", nullable: true),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: true),
                    EmployeeId = table.Column<string>(type: "TEXT", nullable: true),
                    SupplierPaymentID = table.Column<int>(type: "INTEGER", nullable: true),
                    Change = table.Column<string>(type: "TEXT", nullable: true),
                    ExpenseID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Payments_1", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_tbl_Payments_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_SupplierPayment_SupplierPaymentID",
                        column: x => x.SupplierPaymentID,
                        principalTable: "tbl_SupplierPayment",
                        principalColumn: "SupplierPaymentID");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "tbl_Supplier",
                        principalColumn: "SupplierID");
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
                        principalColumn: "depositID");
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
                    productId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    productCode = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    barCode = table.Column<string>(type: "TEXT", unicode: false, maxLength: 50, nullable: true),
                    productName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 150, nullable: true),
                    costExclusive = table.Column<string>(type: "TEXT", nullable: true),
                    costInclusive = table.Column<string>(type: "TEXT", nullable: true),
                    inStock = table.Column<string>(type: "TEXT", nullable: true),
                    priceExclusive = table.Column<string>(type: "TEXT", nullable: true),
                    priceExclusive2 = table.Column<string>(type: "TEXT", nullable: true),
                    priceInclusive = table.Column<string>(type: "TEXT", nullable: true),
                    priceInclusive2 = table.Column<string>(type: "TEXT", nullable: true),
                    categoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    location = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: true),
                    segmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    supplierId = table.Column<int>(type: "INTEGER", nullable: true),
                    productImage = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: true),
                    createdDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    createdBy = table.Column<int>(type: "INTEGER", nullable: false),
                    deleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    trackInventory = table.Column<bool>(type: "INTEGER", nullable: true),
                    ReOrderLevel = table.Column<string>(type: "TEXT", nullable: true),
                    ReOrderQty = table.Column<string>(type: "TEXT", nullable: true),
                    Favourite = table.Column<bool>(type: "INTEGER", nullable: true),
                    hasSubProduct = table.Column<bool>(type: "INTEGER", nullable: true),
                    isAsubProduct = table.Column<int>(type: "INTEGER", nullable: true),
                    compoundCostPricing = table.Column<int>(type: "INTEGER", nullable: true),
                    tax = table.Column<int>(type: "INTEGER", nullable: true),
                    costIncStatus = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_products", x => x.productId);
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
                        principalColumn: "taxID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Expense",
                columns: table => new
                {
                    ExpenseID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerID = table.Column<int>(type: "INTEGER", nullable: true),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: true),
                    EmployeeID = table.Column<string>(type: "TEXT", nullable: true),
                    ExpenseType = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<string>(type: "TEXT", nullable: true),
                    Comment = table.Column<string>(type: "TEXT", unicode: false, maxLength: 300, nullable: true),
                    shiftID = table.Column<int>(type: "INTEGER", nullable: true),
                    dateTimePayed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Expense", x => x.ExpenseID);
                    table.ForeignKey(
                        name: "FK_tbl_Expense_Users_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "tbl_Customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_ExpenseType_ExpenseType",
                        column: x => x.ExpenseType,
                        principalTable: "tbl_ExpenseType",
                        principalColumn: "typeID");
                    table.ForeignKey(
                        name: "FK_tbl_Expense_tbl_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "tbl_Supplier",
                        principalColumn: "SupplierID");
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
                        principalColumn: "shiftId");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ShiftClosureSummaries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentModeID = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleTotal = table.Column<string>(type: "TEXT", nullable: true),
                    TotalCounted = table.Column<string>(type: "TEXT", nullable: true),
                    ShiftExpense = table.Column<string>(type: "TEXT", nullable: true),
                    TotalExpected = table.Column<string>(type: "TEXT", nullable: true),
                    TotalShortage = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
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
                        principalColumn: "shiftId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_transactionDetail",
                columns: table => new
                {
                    detailID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    productID = table.Column<int>(type: "INTEGER", nullable: true),
                    qty = table.Column<string>(type: "TEXT", nullable: true),
                    costExc = table.Column<string>(type: "TEXT", nullable: true),
                    costInc = table.Column<string>(type: "TEXT", nullable: true),
                    priceInc = table.Column<string>(type: "TEXT", nullable: true),
                    priceExc = table.Column<string>(type: "TEXT", nullable: true),
                    taxID = table.Column<int>(type: "INTEGER", nullable: true),
                    taxPercent = table.Column<string>(type: "TEXT", nullable: true),
                    discountID = table.Column<int>(type: "INTEGER", nullable: true),
                    discountPercent = table.Column<string>(type: "TEXT", nullable: true),
                    transactionID = table.Column<int>(type: "INTEGER", nullable: true),
                    totalPriceInc = table.Column<string>(type: "TEXT", nullable: true),
                    totalPriceExc = table.Column<string>(type: "TEXT", nullable: true),
                    sortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    costIncState = table.Column<bool>(type: "INTEGER", nullable: true),
                    specialPricingUsed = table.Column<bool>(type: "INTEGER", nullable: true),
                    tbl_TransactionTransactionId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transactionDetail", x => x.detailID);
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
                        principalColumn: "discountID");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_products_productID",
                        column: x => x.productID,
                        principalTable: "tbl_products",
                        principalColumn: "productId");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_tax_taxID",
                        column: x => x.taxID,
                        principalTable: "tbl_tax",
                        principalColumn: "taxID");
                    table.ForeignKey(
                        name: "FK_tbl_transactionDetail_tbl_transaction_tbl_TransactionTransactionId",
                        column: x => x.tbl_TransactionTransactionId,
                        principalTable: "tbl_transaction",
                        principalColumn: "transactionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_Amount",
                table: "tbl_CashItems",
                column: "Amount",
                unique: true);

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
                column: "ReceiveProductID",
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
                name: "IX_tbl_Tenants_DateTimeCreated",
                table: "tbl_Tenants",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Tenants_DateTimeModified",
                table: "tbl_Tenants",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Tenants_IsDeleted",
                table: "tbl_Tenants",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Tenants_LastModifiedBy",
                table: "tbl_Tenants",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Tenants_TenantId",
                table: "tbl_Tenants",
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
                name: "IX_tbl_transactionDetail_tbl_TransactionTransactionId",
                table: "tbl_transactionDetail",
                column: "tbl_TransactionTransactionId");

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
