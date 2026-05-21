using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddedIscreditnotebooloncustomerDeposittable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCreditNote",
                table: "tbl_customerDeposit",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCreditNote",
                table: "tbl_customerDeposit");
        }
    }
}
