using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class RenameLastNameToCamelCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Lastname",
                table: "Users",
                newName: "LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "Lastname");
        }
    }
}
