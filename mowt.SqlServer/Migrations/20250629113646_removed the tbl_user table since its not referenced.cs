using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class removedthetbl_usertablesinceitsnotreferenced : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    cardNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    col1 = table.Column<bool>(type: "bit", nullable: true),
                    col10 = table.Column<bool>(type: "bit", nullable: true),
                    col11 = table.Column<bool>(type: "bit", nullable: true),
                    col12 = table.Column<bool>(type: "bit", nullable: true),
                    col13 = table.Column<bool>(type: "bit", nullable: true),
                    col14 = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    col15 = table.Column<bool>(type: "bit", nullable: true),
                    col16 = table.Column<bool>(type: "bit", nullable: true),
                    col17 = table.Column<bool>(type: "bit", nullable: true),
                    col18 = table.Column<bool>(type: "bit", nullable: true),
                    col19 = table.Column<bool>(type: "bit", nullable: true),
                    col2 = table.Column<bool>(type: "bit", nullable: true),
                    col20 = table.Column<bool>(type: "bit", nullable: true),
                    col3 = table.Column<bool>(type: "bit", nullable: true),
                    col4 = table.Column<bool>(type: "bit", nullable: true),
                    col5 = table.Column<bool>(type: "bit", nullable: true),
                    col6 = table.Column<bool>(type: "bit", nullable: true),
                    col7 = table.Column<bool>(type: "bit", nullable: true),
                    col8 = table.Column<bool>(type: "bit", nullable: true),
                    col9 = table.Column<bool>(type: "bit", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted = table.Column<int>(type: "int", nullable: true),
                    fullName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    passwordHarsh = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    passwordSalt = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true),
                    profilePic = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    tel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    userName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    userType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_users_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_DateTimeCreated",
                table: "tbl_users",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_DateTimeModified",
                table: "tbl_users",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_IsDeleted",
                table: "tbl_users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_LastModifiedBy",
                table: "tbl_users",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_users_TenantId",
                table: "tbl_users",
                column: "TenantId");
        }
    }
}
