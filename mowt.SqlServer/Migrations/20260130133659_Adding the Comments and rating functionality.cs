using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddingtheCommentsandratingfunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_ProductDetailFeedbacks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductDetailId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FragmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    OriginalContentSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestedContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatingValue = table.Column<int>(type: "int", nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SuggestedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SuggestedByUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuggestedByUserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetailFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbacks_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProductDetailFeedbackReplies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FeedbackId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ParentReplyId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAdminReply = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(36)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProductDetailFeedbackReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbackReplies_tbl_ProductDetailFeedbackReplies_ParentReplyId",
                        column: x => x.ParentReplyId,
                        principalTable: "tbl_ProductDetailFeedbackReplies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbackReplies_tbl_ProductDetailFeedbacks_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "tbl_ProductDetailFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_ProductDetailFeedbackReplies_tbl_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tbl_Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_DateTimeCreated",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_DateTimeModified",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_FeedbackId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_IsDeleted",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_LastModifiedBy",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_ParentReplyId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "ParentReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_TenantId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_DateTimeCreated",
                table: "tbl_ProductDetailFeedbacks",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_DateTimeModified",
                table: "tbl_ProductDetailFeedbacks",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_IsDeleted",
                table: "tbl_ProductDetailFeedbacks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_LastModifiedBy",
                table: "tbl_ProductDetailFeedbacks",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_TenantId",
                table: "tbl_ProductDetailFeedbacks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_ProductDetailFeedbackReplies");

            migrationBuilder.DropTable(
                name: "tbl_ProductDetailFeedbacks");
        }
    }
}
