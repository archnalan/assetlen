using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.SqlServer.mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class ProductAccessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "tbl_products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "tbl_products");
        }
    }
}
