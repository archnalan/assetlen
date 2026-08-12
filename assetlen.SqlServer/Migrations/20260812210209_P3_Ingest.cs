using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <summary>
    /// P3 — the front door (plan.md P3, assetlen.md D3).
    /// <para>
    /// Two new tables and one column. <c>tbl_IngestedMessages</c> is the raw,
    /// immutable record a year of forwarded history lands in;
    /// <c>tbl_IngestBatches</c> is the receipt for each run that put it there.
    /// <c>tbl_Projects_RS.IngestEmailKey</c> addresses the project's inbound
    /// mailbox.
    /// </para>
    /// <para>
    /// <b>Purely additive.</b> Nothing existing is altered or backfilled, because
    /// nothing before P3 wrote ingested material — unlike the P2 migration, which
    /// had to derive sides and owners for rows that predated the model.
    /// </para>
    /// <para>
    /// The load-bearing constraint is <c>UX_IngestedMessage_Project_DedupeKey</c>.
    /// Re-importing an overlapping export must not duplicate, and that guarantee
    /// lives in the schema rather than only in the pre-check that makes it fast.
    /// </para>
    /// </summary>
    public partial class P3_Ingest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IngestEmailKey",
                table: "tbl_Projects_RS",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_IngestBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ArchiveArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    ImportedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ImportedSide = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParsedMessageCount = table.Column<int>(type: "int", nullable: false),
                    ImportedMessageCount = table.Column<int>(type: "int", nullable: false),
                    DuplicateMessageCount = table.Column<int>(type: "int", nullable: false),
                    MediaMessageCount = table.Column<int>(type: "int", nullable: false),
                    NewArtifactCount = table.Column<int>(type: "int", nullable: false),
                    DuplicateArtifactCount = table.Column<int>(type: "int", nullable: false),
                    UnmatchedMediaCount = table.Column<int>(type: "int", nullable: false),
                    ParticipantCount = table.Column<int>(type: "int", nullable: false),
                    FirstMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_IngestBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_AspNetUsers_ImportedById",
                        column: x => x.ImportedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_tbl_Artifacts_ArchiveArtifactId",
                        column: x => x.ArchiveArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestBatches_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_IngestedMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    BatchId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    ExternalAuthor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorMemberId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    MediaFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    IsSystemMessage = table.Column<bool>(type: "bit", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    DedupeKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_IngestedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_IngestBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "tbl_IngestBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_ProjectMembers_AuthorMemberId",
                        column: x => x.AuthorMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_IngestedMessages_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngestBatch_Project_Source",
                table: "tbl_IngestBatches",
                columns: new[] { "ProjectId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_IngestBatch_ProjectId",
                table: "tbl_IngestBatches",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_ArchiveArtifactId",
                table: "tbl_IngestBatches",
                column: "ArchiveArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_DateTimeCreated",
                table: "tbl_IngestBatches",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_DateTimeModified",
                table: "tbl_IngestBatches",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_ImportedById",
                table: "tbl_IngestBatches",
                column: "ImportedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_IsDeleted",
                table: "tbl_IngestBatches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestBatches_LastModifiedBy",
                table: "tbl_IngestBatches",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_IngestedMessage_BatchId",
                table: "tbl_IngestedMessages",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestedMessage_Project_SentAt",
                table: "tbl_IngestedMessages",
                columns: new[] { "ProjectId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_ArtifactId",
                table: "tbl_IngestedMessages",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_AuthorMemberId",
                table: "tbl_IngestedMessages",
                column: "AuthorMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_DateTimeCreated",
                table: "tbl_IngestedMessages",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_DateTimeModified",
                table: "tbl_IngestedMessages",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_IsDeleted",
                table: "tbl_IngestedMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_IngestedMessages_LastModifiedBy",
                table: "tbl_IngestedMessages",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_IngestedMessage_Project_DedupeKey",
                table: "tbl_IngestedMessages",
                columns: new[] { "ProjectId", "DedupeKey" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL AND [DedupeKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_IngestedMessages");

            migrationBuilder.DropTable(
                name: "tbl_IngestBatches");

            migrationBuilder.DropColumn(
                name: "IngestEmailKey",
                table: "tbl_Projects_RS");
        }
    }
}
