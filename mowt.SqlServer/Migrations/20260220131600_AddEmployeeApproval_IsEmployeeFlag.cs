using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeApproval_IsEmployeeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conditionally drop FK only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_tbl_Payments_tbl_customerDeposit_CustomerDepositID'
                      AND parent_object_id = OBJECT_ID('tbl_Payments')
                )
                BEGIN
                    ALTER TABLE [tbl_Payments] DROP CONSTRAINT [FK_tbl_Payments_tbl_customerDeposit_CustomerDepositID];
                END");

            // Conditionally drop tbl_customerDeposit only if it exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.tbl_customerDeposit', 'U') IS NOT NULL
                    DROP TABLE [tbl_customerDeposit];");

            // Conditionally drop tbl_Refunds only if it exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.tbl_Refunds', 'U') IS NOT NULL
                    DROP TABLE [tbl_Refunds];");

            // Conditionally drop index only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_tbl_Payments_CustomerDepositID'
                      AND object_id = OBJECT_ID('tbl_Payments')
                )
                BEGIN
                    DROP INDEX [IX_tbl_Payments_CustomerDepositID] ON [tbl_Payments];
                END");

            // Conditionally drop column only if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE name = 'CustomerDepositID'
                      AND object_id = OBJECT_ID('tbl_Payments')
                )
                BEGIN
                    ALTER TABLE [tbl_Payments] DROP COLUMN [CustomerDepositID];
                END");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployee",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_EmployeeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_EmployeeApprovals_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_EmployeeApprovals");

            migrationBuilder.DropColumn(
                name: "IsEmployee",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "CustomerDepositID",
                table: "tbl_Payments",
                type: "nvarchar(40)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_customerDeposit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    change = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    comment = table.Column<string>(type: "varchar(300)", unicode: false, maxLength: 300, nullable: true),
                    customerID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dateTimeDeposited = table.Column<DateTime>(type: "datetime", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    drawerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCreditNote = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_customerDeposit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Refunds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    refundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    refundComment = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    refundDateTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    refundedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    saleID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    shiftID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    toCustomerID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Refunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_CustomerDepositID",
                table: "tbl_Payments",
                column: "CustomerDepositID");

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
                name: "IX_tbl_Refunds_toCustomerID",
                table: "tbl_Refunds",
                column: "toCustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_tSaleID",
                table: "tbl_Refunds",
                column: "saleID");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Payments_tbl_customerDeposit_CustomerDepositID",
                table: "tbl_Payments",
                column: "CustomerDepositID",
                principalTable: "tbl_customerDeposit",
                principalColumn: "Id");
        }
    }
}
