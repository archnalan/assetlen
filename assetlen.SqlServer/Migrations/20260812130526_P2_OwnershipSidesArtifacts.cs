using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P2_OwnershipSidesArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FloorAreaSqm",
                table: "tbl_Projects_RS",
                type: "decimal(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerTenantId",
                table: "tbl_Projects_RS",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeSource",
                table: "tbl_Projects_RS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeTier",
                table: "tbl_Projects_RS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SizeTierConfirmedAt",
                table: "tbl_Projects_RS",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeTierConfirmedById",
                table: "tbl_Projects_RS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMediator",
                table: "tbl_ProjectMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PartyName",
                table: "tbl_ProjectMembers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "tbl_ProjectMembers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "tbl_ProgressImages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArtifactId",
                table: "tbl_ProgressImages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExposedAt",
                table: "tbl_ProgressImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExposedById",
                table: "tbl_ProgressImages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_Artifacts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ByteSize = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    UploadedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Artifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Artifacts_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Artifacts_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Documents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentRevisionId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Documents_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_TenantMemberships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Roles = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_TenantMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_TenantMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactRefs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ExposedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ExposedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactRefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRefs_AspNetUsers_ExposedById",
                        column: x => x.ExposedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRefs_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ArtifactRevisions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DocumentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RevisionNo = table.Column<int>(type: "int", nullable: false),
                    IssuedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SupersededByRevisionId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ArtifactRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_AspNetUsers_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ArtifactRevisions_tbl_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "tbl_Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_OwnerTenantId",
                table: "tbl_Projects_RS",
                column: "OwnerTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_SizeTier",
                table: "tbl_Projects_RS",
                column: "SizeTier");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_Project_Mediator",
                table: "tbl_ProjectMembers",
                columns: new[] { "ProjectId", "IsMediator" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMember_Project_Side",
                table: "tbl_ProjectMembers",
                columns: new[] { "ProjectId", "Side" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_ArtifactId",
                table: "tbl_ProgressImages",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProgressImages_ExposedById",
                table: "tbl_ProgressImages",
                column: "ExposedById");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_ArtifactId",
                table: "tbl_ArtifactRefs",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_Project_Channel",
                table: "tbl_ArtifactRefs",
                columns: new[] { "ProjectId", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRef_Target",
                table: "tbl_ArtifactRefs",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_DateTimeCreated",
                table: "tbl_ArtifactRefs",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_DateTimeModified",
                table: "tbl_ArtifactRefs",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_ExposedById",
                table: "tbl_ArtifactRefs",
                column: "ExposedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_IsDeleted",
                table: "tbl_ArtifactRefs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRefs_LastModifiedBy",
                table: "tbl_ArtifactRefs",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactRef_Artifact_Target",
                table: "tbl_ArtifactRefs",
                columns: new[] { "ArtifactId", "TargetType", "TargetId" },
                unique: true,
                filter: "[ArtifactId] IS NOT NULL AND [TargetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactRevision_DocumentId",
                table: "tbl_ArtifactRevisions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_ArtifactId",
                table: "tbl_ArtifactRevisions",
                column: "ArtifactId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_DateTimeCreated",
                table: "tbl_ArtifactRevisions",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_DateTimeModified",
                table: "tbl_ArtifactRevisions",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_IsDeleted",
                table: "tbl_ArtifactRevisions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_IssuedById",
                table: "tbl_ArtifactRevisions",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ArtifactRevisions_LastModifiedBy",
                table: "tbl_ArtifactRevisions",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_ArtifactRevision_Document_RevisionNo",
                table: "tbl_ArtifactRevisions",
                columns: new[] { "DocumentId", "RevisionNo" },
                unique: true,
                filter: "[DocumentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_ProjectId",
                table: "tbl_Artifacts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_DateTimeCreated",
                table: "tbl_Artifacts",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_DateTimeModified",
                table: "tbl_Artifacts",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_IsDeleted",
                table: "tbl_Artifacts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_LastModifiedBy",
                table: "tbl_Artifacts",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Artifacts_UploadedById",
                table: "tbl_Artifacts",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "UX_Artifact_Project_Sha256",
                table: "tbl_Artifacts",
                columns: new[] { "ProjectId", "Sha256" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL AND [Sha256] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Document_Kind",
                table: "tbl_Documents",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ProjectId",
                table: "tbl_Documents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_DateTimeCreated",
                table: "tbl_Documents",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_DateTimeModified",
                table: "tbl_Documents",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_IsDeleted",
                table: "tbl_Documents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Documents_LastModifiedBy",
                table: "tbl_Documents",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_DateTimeCreated",
                table: "tbl_TenantMemberships",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_DateTimeModified",
                table: "tbl_TenantMemberships",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_IsDeleted",
                table: "tbl_TenantMemberships",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_TenantMemberships_LastModifiedBy",
                table: "tbl_TenantMemberships",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMembership_UserId",
                table: "tbl_TenantMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_TenantMembership_User_Tenant",
                table: "tbl_TenantMemberships",
                columns: new[] { "UserId", "TenantId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [TenantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProgressImages_AspNetUsers_ExposedById",
                table: "tbl_ProgressImages",
                column: "ExposedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProgressImages_tbl_Artifacts_ArtifactId",
                table: "tbl_ProgressImages",
                column: "ArtifactId",
                principalTable: "tbl_Artifacts",
                principalColumn: "Id");

            // ─── Backfill ────────────────────────────────────────────────
            // Every AddColumn above lands the CLR default, which is wrong for
            // three of them. EF does not apply property initialisers to
            // migrations, so existing rows must be corrected here or they come
            // up misconfigured on the first run.

            // 1. Ownership. Existing projects are owned by the tenant that
            //    already holds them. Without this, OwnerTenantId is null and
            //    child rows fall back to stamping the writer's tenant.
            migrationBuilder.Sql(@"
UPDATE tbl_Projects_RS
SET    OwnerTenantId = TenantId
WHERE  OwnerTenantId IS NULL;");

            // 2. Project sides, from the specialization each member already
            //    holds — mirrors ProjectSideDefaults.For(). ClientOwner (8),
            //    ClientRepresentative (9) and Observer (7) are client-side;
            //    everything else delivers. The column defaulted to 0 (Client),
            //    which would have put every engineer and foreman on the wrong
            //    side of the channel boundary.
            migrationBuilder.Sql(@"
UPDATE tbl_ProjectMembers
SET    Side = CASE WHEN Specialization IN (7, 8, 9) THEN 0 ELSE 1 END;");

            // 3. Mediators — Architect (3) and Lead (0) mediate by default.
            //    A project with no mediator has nobody able to expose anything
            //    to the client side, so the client silently goes dark.
            migrationBuilder.Sql(@"
UPDATE tbl_ProjectMembers
SET    IsMediator = 1
WHERE  Specialization IN (0, 3);");

            // 4. Tenant memberships, from the single tenant each user is bound
            //    to today. AppUser.TenantId survives as the default landing
            //    account; this table becomes the truth about where they may act.
            migrationBuilder.Sql(@"
INSERT INTO tbl_TenantMemberships (Id, UserId, TenantId, IsDefault, IsActive, JoinedAt, DateTimeCreated, IsDeleted)
SELECT      CAST(NEWID() AS nvarchar(40)), u.Id, u.TenantId, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0
FROM        AspNetUsers u
WHERE       u.TenantId IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM tbl_TenantMemberships m
    WHERE  m.UserId = u.Id AND m.TenantId = u.TenantId);");

            // tbl_ProgressImages.Channel went nullable -> NOT NULL DEFAULT 0.
            // Null used to mean "inherit from the parent entry"; it now means
            // Crew. That is the fail-closed direction — an existing frame with
            // no explicit channel becomes crew-only rather than exposed.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProgressImages_AspNetUsers_ExposedById",
                table: "tbl_ProgressImages");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProgressImages_tbl_Artifacts_ArtifactId",
                table: "tbl_ProgressImages");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactRefs");

            migrationBuilder.DropTable(
                name: "tbl_ArtifactRevisions");

            migrationBuilder.DropTable(
                name: "tbl_TenantMemberships");

            migrationBuilder.DropTable(
                name: "tbl_Artifacts");

            migrationBuilder.DropTable(
                name: "tbl_Documents");

            migrationBuilder.DropIndex(
                name: "IX_Project_OwnerTenantId",
                table: "tbl_Projects_RS");

            migrationBuilder.DropIndex(
                name: "IX_Project_SizeTier",
                table: "tbl_Projects_RS");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMember_Project_Mediator",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMember_Project_Side",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProgressImages_ArtifactId",
                table: "tbl_ProgressImages");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProgressImages_ExposedById",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "FloorAreaSqm",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "OwnerTenantId",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "SizeSource",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "SizeTier",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "SizeTierConfirmedAt",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "SizeTierConfirmedById",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "IsMediator",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropColumn(
                name: "PartyName",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropColumn(
                name: "ArtifactId",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "ExposedAt",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "ExposedById",
                table: "tbl_ProgressImages");

            migrationBuilder.AlterColumn<int>(
                name: "Channel",
                table: "tbl_ProgressImages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
