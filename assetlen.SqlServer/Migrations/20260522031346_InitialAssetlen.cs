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
                name: "tbl_Banks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BankName = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    SwiftCode = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Description = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_CashItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CashItems", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_category", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerPricing", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Customers", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExpenseType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExpenseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_location",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Location = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_location", x => x.Id);
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
                name: "tbl_OrderProcesses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<byte[]>(type: "varbinary(150)", maxLength: 150, nullable: true),
                    SortID = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OrderProcesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_OrderStatus",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OrderName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    sortOrder = table.Column<int>(type: "int", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_OrderStatus", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_paymentAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_paymentMode",
                columns: table => new
                {
                    PaymentModeID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_payments", x => x.PaymentModeID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_PrinterPreferances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiptSlipType = table.Column<int>(type: "int", nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PrinterPreferances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductDetailFeedbacks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductDetailId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FragmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OriginalContentSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestedContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatingValue = table.Column<int>(type: "int", nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SuggestedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SuggestedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuggestedByUserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiredApprovals = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetailFeedbacks", x => x.Id);
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
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    creditSupplierAcc = table.Column<bool>(type: "bit", nullable: true),
                    supplierAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    costExc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    costInc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductReceiving", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductRelationships", x => x.Id);
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
                name: "tbl_segment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    segment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    hideInPOS = table.Column<bool>(type: "bit", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_segment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Sizes",
                columns: table => new
                {
                    SizeID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Sizes", x => x.SizeID);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SlipLayout", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Supplier", x => x.Id);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SupplierPayment", x => x.Id);
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
                name: "tbl_tax",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    taxValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    taxDescription = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    deleted = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_tax", x => x.Id);
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
                name: "tbl_UniqueFields",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UniqueField = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UniqueFields", x => x.Id);
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
                    subActiveId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shiftEndCash = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndCard = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndCheque = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    comment = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    drawerStatus = table.Column<bool>(type: "bit", nullable: true),
                    shiftEndBank = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    shiftEndAcc = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_shifts_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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
                    orderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportedId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transactionComment = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_transaction_AspNetUsers_saleAgentID",
                        column: x => x.saleAgentID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transaction_AspNetUsers_soldBy",
                        column: x => x.soldBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_transaction_tbl_Customers_customerId",
                        column: x => x.customerId,
                        principalTable: "tbl_Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_FeedbackApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeedbackId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ApproverUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_tbl_FeedbackApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_FeedbackApprovals_tbl_ProductDetailFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "tbl_ProductDetailFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductDetailFeedbackReplies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeedbackId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ParentReplyId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAdminReply = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetailFeedbackReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbackReplies_tbl_ProductDetailFeedbackReplies_ParentReplyId",
                        column: x => x.ParentReplyId,
                        principalTable: "tbl_ProductDetailFeedbackReplies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbackReplies_tbl_ProductDetailFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "tbl_ProductDetailFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    BankID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    BankingDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    SupplierID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SupplierPaymentID = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    Change = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpenseID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Payments_1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Payments_AspNetUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Payments_tbl_Banks_BankID",
                        column: x => x.BankID,
                        principalTable: "tbl_Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    hasSubProduct = table.Column<bool>(type: "bit", nullable: true),
                    isAsubProduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    compoundCostPricing = table.Column<int>(type: "int", nullable: true),
                    tax = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    costIncStatus = table.Column<bool>(type: "bit", nullable: true),
                    AccessLevel = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_products_tbl_tax_tax",
                        column: x => x.tax,
                        principalTable: "tbl_tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Expense_AspNetUsers_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "AspNetUsers",
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
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ShiftClosureSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ShiftClosureSummaries_tbl_shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "tbl_shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetails_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
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
                    ItemNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tbl_TransactionId = table.Column<string>(type: "nvarchar(40)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transactionDetail", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "tbl_UserDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UserDocuments_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_UserFavorites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UserFavorites_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_tbl_Banks_DateTimeCreated",
                table: "tbl_Banks",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Banks_DateTimeModified",
                table: "tbl_Banks",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Banks_IsDeleted",
                table: "tbl_Banks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Banks_LastModifiedBy",
                table: "tbl_Banks",
                column: "LastModifiedBy");

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
                name: "IX_tbl_FeedbackApprovals_DateTimeCreated",
                table: "tbl_FeedbackApprovals",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_DateTimeModified",
                table: "tbl_FeedbackApprovals",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_FeedbackId",
                table: "tbl_FeedbackApprovals",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_IsDeleted",
                table: "tbl_FeedbackApprovals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_LastModifiedBy",
                table: "tbl_FeedbackApprovals",
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
                name: "IX_tbl_Payments_BankID",
                table: "tbl_Payments",
                column: "BankID");

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
                name: "IX_tbl_PrinterPreferances_DateTimeCreated",
                table: "tbl_PrinterPreferances",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PrinterPreferances_DateTimeModified",
                table: "tbl_PrinterPreferances",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PrinterPreferances_IsDeleted",
                table: "tbl_PrinterPreferances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PrinterPreferances_LastModifiedBy",
                table: "tbl_PrinterPreferances",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_DateTimeCreated",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_DateTimeModified",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_FeedbackId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_IsDeleted",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_LastModifiedBy",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_ParentReplyId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "ParentReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_DateTimeCreated",
                table: "tbl_ProductDetailFeedbacks",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_DateTimeModified",
                table: "tbl_ProductDetailFeedbacks",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_IsDeleted",
                table: "tbl_ProductDetailFeedbacks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_LastModifiedBy",
                table: "tbl_ProductDetailFeedbacks",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_DateTimeCreated",
                table: "tbl_ProductDetails",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_DateTimeModified",
                table: "tbl_ProductDetails",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_IsDeleted",
                table: "tbl_ProductDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_LastModifiedBy",
                table: "tbl_ProductDetails",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_ProductId",
                table: "tbl_ProductDetails",
                column: "ProductId");

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
                name: "IX_tbl_UserDocuments_DateTimeCreated",
                table: "tbl_UserDocuments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_DateTimeModified",
                table: "tbl_UserDocuments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_IsDeleted",
                table: "tbl_UserDocuments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_LastModifiedBy",
                table: "tbl_UserDocuments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_ProductId",
                table: "tbl_UserDocuments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_DateTimeCreated",
                table: "tbl_UserFavorites",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_DateTimeModified",
                table: "tbl_UserFavorites",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_IsDeleted",
                table: "tbl_UserFavorites",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_LastModifiedBy",
                table: "tbl_UserFavorites",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_ProductId",
                table: "tbl_UserFavorites",
                column: "ProductId");
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
                name: "tbl_CashItems");

            migrationBuilder.DropTable(
                name: "tbl_category");

            migrationBuilder.DropTable(
                name: "tbl_Configuration");

            migrationBuilder.DropTable(
                name: "tbl_customerPricing");

            migrationBuilder.DropTable(
                name: "tbl_EmployeeApprovals");

            migrationBuilder.DropTable(
                name: "tbl_Expense");

            migrationBuilder.DropTable(
                name: "tbl_FeedbackApprovals");

            migrationBuilder.DropTable(
                name: "tbl_Flags");

            migrationBuilder.DropTable(
                name: "tbl_FundingEntries");

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
                name: "tbl_PrinterPreferances");

            migrationBuilder.DropTable(
                name: "tbl_ProductDetailFeedbackReplies");

            migrationBuilder.DropTable(
                name: "tbl_ProductDetails");

            migrationBuilder.DropTable(
                name: "tbl_ProductReceiving");

            migrationBuilder.DropTable(
                name: "tbl_ProductRelationships");

            migrationBuilder.DropTable(
                name: "tbl_ProgressComments");

            migrationBuilder.DropTable(
                name: "tbl_ProjectSubscriptions");

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
                name: "tbl_SubscriptionSeats");

            migrationBuilder.DropTable(
                name: "tbl_SyncLogs");

            migrationBuilder.DropTable(
                name: "tbl_transactionDetail");

            migrationBuilder.DropTable(
                name: "tbl_UniqueFields");

            migrationBuilder.DropTable(
                name: "tbl_UserDocuments");

            migrationBuilder.DropTable(
                name: "tbl_UserFavorites");

            migrationBuilder.DropTable(
                name: "VerificationCodes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "tbl_Tenants");

            migrationBuilder.DropTable(
                name: "tbl_ExpenseType");

            migrationBuilder.DropTable(
                name: "tbl_Banks");

            migrationBuilder.DropTable(
                name: "tbl_SupplierPayment");

            migrationBuilder.DropTable(
                name: "tbl_Supplier");

            migrationBuilder.DropTable(
                name: "tbl_paymentMode");

            migrationBuilder.DropTable(
                name: "tbl_ProductDetailFeedbacks");

            migrationBuilder.DropTable(
                name: "tbl_ProgressImages");

            migrationBuilder.DropTable(
                name: "tbl_shifts");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionRequests");

            migrationBuilder.DropTable(
                name: "tbl_discounts");

            migrationBuilder.DropTable(
                name: "tbl_transaction");

            migrationBuilder.DropTable(
                name: "tbl_products");

            migrationBuilder.DropTable(
                name: "tbl_ProgressUpdates");

            migrationBuilder.DropTable(
                name: "tbl_Customers");

            migrationBuilder.DropTable(
                name: "tbl_tax");

            migrationBuilder.DropTable(
                name: "tbl_Stages");

            migrationBuilder.DropTable(
                name: "tbl_Projects_RS");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
