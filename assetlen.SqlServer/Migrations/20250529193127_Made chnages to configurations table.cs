using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class Madechnagestoconfigurationstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_Configuration",
                table: "tbl_Configuration");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Configuration_DateTimeCreated",
                table: "tbl_Configuration");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Configuration_DateTimeModified",
                table: "tbl_Configuration");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Configuration_IsDeleted",
                table: "tbl_Configuration");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Configuration_LastModifiedBy",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "tbl_Configuration",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "LastModifiedBy",
                table: "tbl_Configuration",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_Configuration",
                table: "tbl_Configuration",
                column: "SettingID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
