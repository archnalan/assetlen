using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Knockoff_WorkPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "tbl_Deliverables",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionArtifactId",
                table: "tbl_Deliverables",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEnd",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStart",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Trade",
                table: "tbl_Deliverables",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkDays",
                table: "tbl_Deliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_DeliverableEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ArtifactId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ProgressUpdateId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DeliverableEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_DeliverableEvents_AspNetUsers_ById",
                        column: x => x.ById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_DeliverableEvents_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliverable_Project_PlannedStart",
                table: "tbl_Deliverables",
                columns: new[] { "ProjectId", "PlannedStart" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_CompletionArtifactId",
                table: "tbl_Deliverables",
                column: "CompletionArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliverableEvent_DeliverableId",
                table: "tbl_DeliverableEvents",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DeliverableEvents_ById",
                table: "tbl_DeliverableEvents",
                column: "ById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DeliverableEvents_DateTimeCreated",
                table: "tbl_DeliverableEvents",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DeliverableEvents_DateTimeModified",
                table: "tbl_DeliverableEvents",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DeliverableEvents_IsDeleted",
                table: "tbl_DeliverableEvents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DeliverableEvents_LastModifiedBy",
                table: "tbl_DeliverableEvents",
                column: "LastModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Deliverables_tbl_Artifacts_CompletionArtifactId",
                table: "tbl_Deliverables",
                column: "CompletionArtifactId",
                principalTable: "tbl_Artifacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Deliverables_tbl_Artifacts_CompletionArtifactId",
                table: "tbl_Deliverables");

            migrationBuilder.DropTable(
                name: "tbl_DeliverableEvents");

            migrationBuilder.DropIndex(
                name: "IX_Deliverable_Project_PlannedStart",
                table: "tbl_Deliverables");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Deliverables_CompletionArtifactId",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "CompletionArtifactId",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "PlannedEnd",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "PlannedStart",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "Trade",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "WorkDays",
                table: "tbl_Deliverables");
        }
    }
}
