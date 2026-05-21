using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredApprovals",
                table: "tbl_ProductDetailFeedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tbl_FeedbackApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeedbackId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ApproverUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FeedbackApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_FeedbackApprovals_tbl_ProductDetailFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "tbl_ProductDetailFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_FeedbackApprovals_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_DateTimeCreated",
                table: "tbl_FeedbackApprovals",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_DateTimeModified",
                table: "tbl_FeedbackApprovals",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_FeedbackId",
                table: "tbl_FeedbackApprovals",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_IsDeleted",
                table: "tbl_FeedbackApprovals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_LastModifiedBy",
                table: "tbl_FeedbackApprovals",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_TenantId",
                table: "tbl_FeedbackApprovals",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_FeedbackApprovals");

            migrationBuilder.DropColumn(
                name: "RequiredApprovals",
                table: "tbl_ProductDetailFeedbacks");
        }
    }
}
