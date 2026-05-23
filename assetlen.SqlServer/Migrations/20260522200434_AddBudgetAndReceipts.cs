using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAndReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_Receipts");

            migrationBuilder.DropTable(
                name: "tbl_BudgetLineItems");
        }
    }
}
