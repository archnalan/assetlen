using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class R_WorksReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReportDraftingEnabled",
                table: "tbl_Projects_RS",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RaisedAt",
                table: "tbl_Flags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactPosters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    PosterArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DurationSeconds = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactPosters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactPosters_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactPosters_tbl_Artifacts_PosterArtifactId",
                        column: x => x.PosterArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_WorksReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WindowFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audience = table.Column<int>(type: "int", nullable: false),
                    IssuedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssueKind = table.Column<int>(type: "int", nullable: false),
                    IssueReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TriggerKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CoveringNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NarrativeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PreviousReportId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DeliveryNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_WorksReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_WorksReports_AspNetUsers_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_WorksReports_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_DateTimeCreated",
                table: "tbl_ArtifactPosters",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_DateTimeModified",
                table: "tbl_ArtifactPosters",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_IsDeleted",
                table: "tbl_ArtifactPosters",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_LastModifiedBy",
                table: "tbl_ArtifactPosters",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactPosters_PosterArtifactId",
                table: "tbl_ArtifactPosters",
                column: "PosterArtifactId");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactPoster_ArtifactId",
                table: "tbl_ArtifactPosters",
                column: "ArtifactId",
                unique: true,
                filter: "[ArtifactId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_DateTimeCreated",
                table: "tbl_WorksReports",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_DateTimeModified",
                table: "tbl_WorksReports",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_IsDeleted",
                table: "tbl_WorksReports",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_IssuedById",
                table: "tbl_WorksReports",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorksReports_LastModifiedBy",
                table: "tbl_WorksReports",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorksReport_Project_AsAt",
                table: "tbl_WorksReports",
                columns: new[] { "ProjectId", "AsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorksReport_Project_Trigger",
                table: "tbl_WorksReports",
                columns: new[] { "ProjectId", "TriggerKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ArtifactPosters");

            migrationBuilder.DropTable(
                name: "tbl_WorksReports");

            migrationBuilder.DropColumn(
                name: "ReportDraftingEnabled",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "RaisedAt",
                table: "tbl_Flags");
        }
    }
}
