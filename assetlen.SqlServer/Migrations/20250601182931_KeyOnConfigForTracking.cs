using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class KeyOnConfigForTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "tbl_Configuration",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_Configuration",
                table: "tbl_Configuration",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_Configuration",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "tbl_Configuration",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);
        }
    }
}
