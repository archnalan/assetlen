using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class BanksCRUD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BankID",
                table: "tbl_Payments",
                type: "nvarchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Banks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Banks_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_BankID",
                table: "tbl_Payments",
                column: "BankID");

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
                name: "IX_tbl_Banks_TenantId",
                table: "tbl_Banks",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Payments_tbl_Banks_BankID",
                table: "tbl_Payments",
                column: "BankID",
                principalTable: "tbl_Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Payments_tbl_Banks_BankID",
                table: "tbl_Payments");

            migrationBuilder.DropTable(
                name: "tbl_Banks");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Payments_BankID",
                table: "tbl_Payments");

            migrationBuilder.AlterColumn<string>(
                name: "BankID",
                table: "tbl_Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldNullable: true);
        }
    }
}
