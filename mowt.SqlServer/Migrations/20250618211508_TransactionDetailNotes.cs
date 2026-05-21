using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class TransactionDetailNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemNote",
                table: "tbl_transactionDetail",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemNote",
                table: "tbl_transactionDetail");
        }
    }
}
