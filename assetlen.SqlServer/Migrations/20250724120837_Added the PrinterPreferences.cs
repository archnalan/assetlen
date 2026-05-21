using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddedthePrinterPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_PrinterPreferances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiptSlipType = table.Column<int>(type: "int", nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PrinterPreferances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PrinterPreferances_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_tbl_PrinterPreferances_TenantId",
                table: "tbl_PrinterPreferances",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_PrinterPreferances");
        }
    }
}
