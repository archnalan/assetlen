using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class ProductDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_ProductDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetails_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetails_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_DateTimeCreated",
                table: "tbl_ProductDetails",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_DateTimeModified",
                table: "tbl_ProductDetails",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_IsDeleted",
                table: "tbl_ProductDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_LastModifiedBy",
                table: "tbl_ProductDetails",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_ProductId",
                table: "tbl_ProductDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_TenantId",
                table: "tbl_ProductDetails",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ProductDetails");
        }
    }
}
