using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P8_MarkupAndParkedIdeas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DependsOnStageId",
                table: "tbl_Commitments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_Annotations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LayerId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuthorSide = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    ShapesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommitmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Annotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_tbl_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalTable: "tbl_Artifacts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Annotations_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_CommitmentEstimates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IngestedMessageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RecordedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CommitmentEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentEstimates_AspNetUsers_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentEstimates_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DependsOnStageId",
                table: "tbl_Commitments",
                column: "DependsOnStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotation_CommitmentId",
                table: "tbl_Annotations",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotation_ProjectId",
                table: "tbl_Annotations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_AuthorId",
                table: "tbl_Annotations",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_DateTimeCreated",
                table: "tbl_Annotations",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_DateTimeModified",
                table: "tbl_Annotations",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_IsDeleted",
                table: "tbl_Annotations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Annotations_LastModifiedBy",
                table: "tbl_Annotations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "UX_Annotation_Artifact_Layer_Version",
                table: "tbl_Annotations",
                columns: new[] { "ArtifactId", "LayerId", "Version" },
                unique: true,
                filter: "[ArtifactId] IS NOT NULL AND [LayerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentEstimate_CommitmentId",
                table: "tbl_CommitmentEstimates",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_DateTimeCreated",
                table: "tbl_CommitmentEstimates",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_DateTimeModified",
                table: "tbl_CommitmentEstimates",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_IsDeleted",
                table: "tbl_CommitmentEstimates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_LastModifiedBy",
                table: "tbl_CommitmentEstimates",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentEstimates_RecordedById",
                table: "tbl_CommitmentEstimates",
                column: "RecordedById");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Commitments_tbl_Stages_DependsOnStageId",
                table: "tbl_Commitments",
                column: "DependsOnStageId",
                principalTable: "tbl_Stages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Commitments_tbl_Stages_DependsOnStageId",
                table: "tbl_Commitments");

            migrationBuilder.DropTable(
                name: "tbl_Annotations");

            migrationBuilder.DropTable(
                name: "tbl_CommitmentEstimates");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Commitments_DependsOnStageId",
                table: "tbl_Commitments");

            migrationBuilder.DropColumn(
                name: "DependsOnStageId",
                table: "tbl_Commitments");
        }
    }
}
