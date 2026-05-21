using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class removedUniqueIndexFromCashItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tbl_CashItems_Amount",
                table: "tbl_CashItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_Amount",
                table: "tbl_CashItems",
                column: "Amount",
                unique: true,
                filter: "[Amount] IS NOT NULL");
        }
    }
}
