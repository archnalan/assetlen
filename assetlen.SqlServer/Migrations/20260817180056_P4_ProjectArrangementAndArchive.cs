using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P4_ProjectArrangementAndArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "tbl_Projects_RS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedById",
                table: "tbl_Projects_RS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_ProjectPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    PinnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProjectPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProjectPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectPreferences_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_ArchivedAt",
                table: "tbl_Projects_RS",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPreference_User_Pinned",
                table: "tbl_ProjectPreferences",
                columns: new[] { "UserId", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPreference_User_Project",
                table: "tbl_ProjectPreferences",
                columns: new[] { "UserId", "ProjectId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [ProjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_DateTimeCreated",
                table: "tbl_ProjectPreferences",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_DateTimeModified",
                table: "tbl_ProjectPreferences",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_IsDeleted",
                table: "tbl_ProjectPreferences",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_LastModifiedBy",
                table: "tbl_ProjectPreferences",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProjectPreferences_ProjectId",
                table: "tbl_ProjectPreferences",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ProjectPreferences");

            migrationBuilder.DropIndex(
                name: "IX_Project_ArchivedAt",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "ArchivedById",
                table: "tbl_Projects_RS");
        }
    }
}
