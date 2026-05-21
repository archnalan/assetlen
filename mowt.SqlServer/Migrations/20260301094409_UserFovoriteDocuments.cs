using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class UserFovoriteDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_UserDocuments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UserDocuments_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_UserFavorites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_UserFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_UserFavorites_tbl_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "tbl_products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_DateTimeCreated",
                table: "tbl_UserDocuments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_DateTimeModified",
                table: "tbl_UserDocuments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_IsDeleted",
                table: "tbl_UserDocuments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_LastModifiedBy",
                table: "tbl_UserDocuments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserDocuments_ProductId",
                table: "tbl_UserDocuments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_DateTimeCreated",
                table: "tbl_UserFavorites",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_DateTimeModified",
                table: "tbl_UserFavorites",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_IsDeleted",
                table: "tbl_UserFavorites",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_LastModifiedBy",
                table: "tbl_UserFavorites",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UserFavorites_ProductId",
                table: "tbl_UserFavorites",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_UserDocuments");

            migrationBuilder.DropTable(
                name: "tbl_UserFavorites");
        }
    }
}
