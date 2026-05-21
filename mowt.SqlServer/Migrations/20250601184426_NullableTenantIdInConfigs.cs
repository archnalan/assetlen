using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class NullableTenantIdInConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId1",
                table: "tbl_Configuration",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration",
                column: "TenantId1",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId1",
                table: "tbl_Configuration",
                type: "nvarchar(36)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration",
                column: "TenantId1",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
