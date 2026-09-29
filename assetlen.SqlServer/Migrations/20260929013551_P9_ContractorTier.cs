using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P9_ContractorTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CapturedAt",
                table: "tbl_ProgressUpdates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientCaptureId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliverableId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoiceArtifactId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CuratedAt",
                table: "tbl_ProgressImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CuratedById",
                table: "tbl_ProgressImages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Curation",
                table: "tbl_ProgressImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tbl_BriefPublications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Trigger = table.Column<int>(type: "int", nullable: false),
                    PublishedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FramesExposed = table.Column<int>(type: "int", nullable: false),
                    FramesDropped = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_BriefPublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_BriefPublications_AspNetUsers_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_BriefPublications_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ClaimEvidence",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ClaimId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    ProgressImageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProgressReadingId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ClaimEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_ProgressImages_ProgressImageId",
                        column: x => x.ProgressImageId,
                        principalTable: "tbl_ProgressImages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_ProgressReadings_ProgressReadingId",
                        column: x => x.ProgressReadingId,
                        principalTable: "tbl_ProgressReadings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ClaimEvidence_tbl_StageClaims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "tbl_StageClaims",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EndpointHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    P256dh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Auth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LastSuccessAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PushSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PushSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_PushDeliveries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SubscriptionId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HttpStatus = table.Column<int>(type: "int", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PushDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PushDeliveries_tbl_PushSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "tbl_PushSubscriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressUpdate_DeliverableId",
                table: "tbl_ProgressUpdates",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressUpdates_VoiceArtifactId",
                table: "tbl_ProgressUpdates",
                column: "VoiceArtifactId");

            migrationBuilder.CreateIndex(
                name: "UX_ProgressUpdate_Project_ClientCapture",
                table: "tbl_ProgressUpdates",
                columns: new[] { "ProjectId", "ClientCaptureId" },
                unique: true,
                filter: "[ClientCaptureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_DateTimeCreated",
                table: "tbl_BriefPublications",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_DateTimeModified",
                table: "tbl_BriefPublications",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_IsDeleted",
                table: "tbl_BriefPublications",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_LastModifiedBy",
                table: "tbl_BriefPublications",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_BriefPublications_PublishedById",
                table: "tbl_BriefPublications",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "UX_BriefPublication_Project_Day",
                table: "tbl_BriefPublications",
                columns: new[] { "ProjectId", "Day" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimEvidence_ClaimId",
                table: "tbl_ClaimEvidence",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ArtifactId",
                table: "tbl_ClaimEvidence",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DateTimeCreated",
                table: "tbl_ClaimEvidence",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DateTimeModified",
                table: "tbl_ClaimEvidence",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_DeliverableId",
                table: "tbl_ClaimEvidence",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_IsDeleted",
                table: "tbl_ClaimEvidence",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_LastModifiedBy",
                table: "tbl_ClaimEvidence",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ProgressImageId",
                table: "tbl_ClaimEvidence",
                column: "ProgressImageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ClaimEvidence_ProgressReadingId",
                table: "tbl_ClaimEvidence",
                column: "ProgressReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_PushDelivery_Status",
                table: "tbl_PushDeliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PushDelivery_User_QueuedAt",
                table: "tbl_PushDeliveries",
                columns: new[] { "UserId", "QueuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_DateTimeCreated",
                table: "tbl_PushDeliveries",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_DateTimeModified",
                table: "tbl_PushDeliveries",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_IsDeleted",
                table: "tbl_PushDeliveries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_LastModifiedBy",
                table: "tbl_PushDeliveries",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushDeliveries_SubscriptionId",
                table: "tbl_PushDeliveries",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_UserId",
                table: "tbl_PushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_DateTimeCreated",
                table: "tbl_PushSubscriptions",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_DateTimeModified",
                table: "tbl_PushSubscriptions",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_IsDeleted",
                table: "tbl_PushSubscriptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PushSubscriptions_LastModifiedBy",
                table: "tbl_PushSubscriptions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_PushSubscription_EndpointHash",
                table: "tbl_PushSubscriptions",
                column: "EndpointHash",
                unique: true,
                filter: "[EndpointHash] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProgressUpdates_tbl_Artifacts_VoiceArtifactId",
                table: "tbl_ProgressUpdates",
                column: "VoiceArtifactId",
                principalTable: "tbl_Artifacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProgressUpdates_tbl_Deliverables_DeliverableId",
                table: "tbl_ProgressUpdates",
                column: "DeliverableId",
                principalTable: "tbl_Deliverables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProgressUpdates_tbl_Artifacts_VoiceArtifactId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProgressUpdates_tbl_Deliverables_DeliverableId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropTable(
                name: "tbl_BriefPublications");

            migrationBuilder.DropTable(
                name: "tbl_ClaimEvidence");

            migrationBuilder.DropTable(
                name: "tbl_PushDeliveries");

            migrationBuilder.DropTable(
                name: "tbl_PushSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ProgressUpdate_DeliverableId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProgressUpdates_VoiceArtifactId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropIndex(
                name: "UX_ProgressUpdate_Project_ClientCapture",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "CapturedAt",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "ClientCaptureId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "DeliverableId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "VoiceArtifactId",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "CuratedAt",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "CuratedById",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "Curation",
                table: "tbl_ProgressImages");
        }
    }
}
