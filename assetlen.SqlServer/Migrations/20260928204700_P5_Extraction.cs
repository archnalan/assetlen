using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P5_Extraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoundMediaCount",
                table: "tbl_IngestBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactTexts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CharCount = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    ExtractedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactTexts_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExtractionProposals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RunId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Maturity = table.Column<int>(type: "int", nullable: false),
                    OwedBySide = table.Column<int>(type: "int", nullable: true),
                    PartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Rule = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Engine = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    Contested = table.Column<bool>(type: "bit", nullable: false),
                    ContestNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommitmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FlagId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceSide = table.Column<int>(type: "int", nullable: false),
                    SourceImportedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SourceSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExtractionProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_AspNetUsers_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_tbl_IngestedMessages_IngestedMessageId",
                        column: x => x.IngestedMessageId,
                        principalTable: "tbl_IngestedMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ExtractionProposals_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ExtractionRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StartedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Engine = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MessagesRead = table.Column<int>(type: "int", nullable: false),
                    ProposalsCreated = table.Column<int>(type: "int", nullable: false),
                    ReadingsCreated = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ExtractionRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_MediaBindings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BatchId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    StampedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MediaBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MediaBindings_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_MediaBindings_tbl_IngestedMessages_IngestedMessageId",
                        column: x => x.IngestedMessageId,
                        principalTable: "tbl_IngestedMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressReadings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceKind = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceSide = table.Column<int>(type: "int", nullable: true),
                    SourceImportedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressReadings_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            // Each existing stage starts its history with the figure it holds now —
            // the honest best available; readings from before today were never kept.
            migrationBuilder.Sql(@"
INSERT INTO tbl_ProgressReadings (Id, ProjectId, StageId, Subject, [Percent], ObservedAt, SourceKind, SourceId, IsDeleted, TenantId, DateTimeCreated)
SELECT CONVERT(nvarchar(40), NEWID()), s.ProjectId, s.Id, LEFT(s.StageName, 200),
       CASE WHEN s.CompletionPercentage > 100 THEN 100 ELSE s.CompletionPercentage END,
       COALESCE(s.DateTimeModified, s.DateTimeCreated, SYSUTCDATETIME()), 0, s.Id, 0, s.TenantId, SYSUTCDATETIME()
FROM tbl_Stages s
WHERE s.CompletionPercentage IS NOT NULL AND s.CompletionPercentage > 0 AND ISNULL(s.IsDeleted, 0) = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactText_Project_Status",
                table: "tbl_ArtifactTexts",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_DateTimeCreated",
                table: "tbl_ArtifactTexts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_DateTimeModified",
                table: "tbl_ArtifactTexts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_IsDeleted",
                table: "tbl_ArtifactTexts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactTexts_LastModifiedBy",
                table: "tbl_ArtifactTexts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactText_ArtifactId",
                table: "tbl_ArtifactTexts",
                column: "ArtifactId",
                unique: true,
                filter: "[ArtifactId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionProposal_MessageId",
                table: "tbl_ExtractionProposals",
                column: "IngestedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionProposal_Project_Status",
                table: "tbl_ExtractionProposals",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DateTimeCreated",
                table: "tbl_ExtractionProposals",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DateTimeModified",
                table: "tbl_ExtractionProposals",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_DecidedById",
                table: "tbl_ExtractionProposals",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_IsDeleted",
                table: "tbl_ExtractionProposals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_LastModifiedBy",
                table: "tbl_ExtractionProposals",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionProposals_StageId",
                table: "tbl_ExtractionProposals",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "UX_ExtractionProposal_Project_Fingerprint",
                table: "tbl_ExtractionProposals",
                columns: new[] { "ProjectId", "Fingerprint" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL AND [Fingerprint] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionRun_ProjectId",
                table: "tbl_ExtractionRuns",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_DateTimeCreated",
                table: "tbl_ExtractionRuns",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_DateTimeModified",
                table: "tbl_ExtractionRuns",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_IsDeleted",
                table: "tbl_ExtractionRuns",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExtractionRuns_LastModifiedBy",
                table: "tbl_ExtractionRuns",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MediaBinding_Project_Artifact",
                table: "tbl_MediaBindings",
                columns: new[] { "ProjectId", "ArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_ArtifactId",
                table: "tbl_MediaBindings",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_DateTimeCreated",
                table: "tbl_MediaBindings",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_DateTimeModified",
                table: "tbl_MediaBindings",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_IsDeleted",
                table: "tbl_MediaBindings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MediaBindings_LastModifiedBy",
                table: "tbl_MediaBindings",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_MediaBinding_MessageId",
                table: "tbl_MediaBindings",
                column: "IngestedMessageId",
                unique: true,
                filter: "[IngestedMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReading_Project_ObservedAt",
                table: "tbl_ProgressReadings",
                columns: new[] { "ProjectId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReading_Stage_ObservedAt",
                table: "tbl_ProgressReadings",
                columns: new[] { "StageId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_DateTimeCreated",
                table: "tbl_ProgressReadings",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_DateTimeModified",
                table: "tbl_ProgressReadings",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_IsDeleted",
                table: "tbl_ProgressReadings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressReadings_LastModifiedBy",
                table: "tbl_ProgressReadings",
                column: "LastModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ArtifactTexts");

            migrationBuilder.DropTable(
                name: "tbl_ExtractionProposals");

            migrationBuilder.DropTable(
                name: "tbl_ExtractionRuns");

            migrationBuilder.DropTable(
                name: "tbl_MediaBindings");

            migrationBuilder.DropTable(
                name: "tbl_ProgressReadings");

            migrationBuilder.DropColumn(
                name: "BoundMediaCount",
                table: "tbl_IngestBatches");
        }
    }
}
