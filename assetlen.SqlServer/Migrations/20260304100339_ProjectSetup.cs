using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class ProjectSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_Projects_RS",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ExpectedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvestorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProjectManagerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsFirstFreeProject = table.Column<bool>(type: "bit", nullable: false),
                    IsSubscriptionActive = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Projects_RS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Projects_RS_Users_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Projects_RS_Users_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Stages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Stages_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProjectSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    InvestorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProjectSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProjectSubscriptions_Users_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProjectSubscriptions_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_FundingEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ConfirmedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FundingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_Users_ConfirmedById",
                        column: x => x.ConfirmedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_Users_PaidById",
                        column: x => x.PaidById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_FundingEntries_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressUpdates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CompletionPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HasIssues = table.Column<bool>(type: "bit", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressUpdates_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressImages_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_ProgressComments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProgressUpdateId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ProgressImageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CommentText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ParentCommentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ProgressComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "tbl_ProgressComments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressImages_ProgressImageId",
                        column: x => x.ProgressImageId,
                        principalTable: "tbl_ProgressImages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_ProgressComments_tbl_ProgressUpdates_ProgressUpdateId",
                        column: x => x.ProgressUpdateId,
                        principalTable: "tbl_ProgressUpdates",
                        principalColumn: "Id");
                });

            // Indexes for tbl_Projects_RS
            migrationBuilder.CreateIndex(name: "IX_tbl_Projects_RS_DateTimeCreated", table: "tbl_Projects_RS", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_Projects_RS_DateTimeModified", table: "tbl_Projects_RS", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_Project_InvestorId", table: "tbl_Projects_RS", column: "InvestorId");
            migrationBuilder.CreateIndex(name: "IX_tbl_Projects_RS_IsDeleted", table: "tbl_Projects_RS", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_Projects_RS_LastModifiedBy", table: "tbl_Projects_RS", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_Project_ProjectManagerId", table: "tbl_Projects_RS", column: "ProjectManagerId");
            migrationBuilder.CreateIndex(name: "IX_Project_Status", table: "tbl_Projects_RS", column: "Status");

            // Indexes for tbl_Stages
            migrationBuilder.CreateIndex(name: "IX_tbl_Stages_DateTimeCreated", table: "tbl_Stages", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_Stages_DateTimeModified", table: "tbl_Stages", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_tbl_Stages_IsDeleted", table: "tbl_Stages", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_Stages_LastModifiedBy", table: "tbl_Stages", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_Stage_ProjectId", table: "tbl_Stages", column: "ProjectId");

            // Indexes for tbl_ProjectSubscriptions
            migrationBuilder.CreateIndex(name: "IX_tbl_ProjectSubscriptions_DateTimeCreated", table: "tbl_ProjectSubscriptions", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProjectSubscriptions_DateTimeModified", table: "tbl_ProjectSubscriptions", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_ProjectSub_InvestorId", table: "tbl_ProjectSubscriptions", column: "InvestorId");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProjectSubscriptions_IsDeleted", table: "tbl_ProjectSubscriptions", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProjectSubscriptions_LastModifiedBy", table: "tbl_ProjectSubscriptions", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_ProjectSub_ProjectId", table: "tbl_ProjectSubscriptions", column: "ProjectId");

            // Indexes for tbl_FundingEntries
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_ConfirmedById", table: "tbl_FundingEntries", column: "ConfirmedById");
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_DateTimeCreated", table: "tbl_FundingEntries", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_DateTimeModified", table: "tbl_FundingEntries", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_IsDeleted", table: "tbl_FundingEntries", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_LastModifiedBy", table: "tbl_FundingEntries", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_tbl_FundingEntries_PaidById", table: "tbl_FundingEntries", column: "PaidById");
            migrationBuilder.CreateIndex(name: "IX_FundingEntry_ProjectId", table: "tbl_FundingEntries", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_FundingEntry_StageId", table: "tbl_FundingEntries", column: "StageId");
            migrationBuilder.CreateIndex(name: "IX_FundingEntry_Status", table: "tbl_FundingEntries", column: "Status");

            // Indexes for tbl_ProgressUpdates
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressUpdates_CreatedById", table: "tbl_ProgressUpdates", column: "CreatedById");
            migrationBuilder.CreateIndex(name: "IX_ProgressUpdate_CreatedAt", table: "tbl_ProgressUpdates", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressUpdates_DateTimeModified", table: "tbl_ProgressUpdates", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressUpdates_IsDeleted", table: "tbl_ProgressUpdates", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressUpdates_LastModifiedBy", table: "tbl_ProgressUpdates", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_ProgressUpdate_ProjectId", table: "tbl_ProgressUpdates", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_ProgressUpdate_StageId", table: "tbl_ProgressUpdates", column: "StageId");

            // Indexes for tbl_ProgressImages
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressImages_DateTimeCreated", table: "tbl_ProgressImages", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressImages_DateTimeModified", table: "tbl_ProgressImages", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressImages_IsDeleted", table: "tbl_ProgressImages", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressImages_LastModifiedBy", table: "tbl_ProgressImages", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_ProgressImage_UpdateId", table: "tbl_ProgressImages", column: "ProgressUpdateId");

            // Indexes for tbl_ProgressComments
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_AuthorId", table: "tbl_ProgressComments", column: "AuthorId");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_DateTimeCreated", table: "tbl_ProgressComments", column: "DateTimeCreated");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_DateTimeModified", table: "tbl_ProgressComments", column: "DateTimeModified");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_IsDeleted", table: "tbl_ProgressComments", column: "IsDeleted");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_LastModifiedBy", table: "tbl_ProgressComments", column: "LastModifiedBy");
            migrationBuilder.CreateIndex(name: "IX_tbl_ProgressComments_ParentCommentId", table: "tbl_ProgressComments", column: "ParentCommentId");
            migrationBuilder.CreateIndex(name: "IX_ProgressComment_ImageId", table: "tbl_ProgressComments", column: "ProgressImageId");
            migrationBuilder.CreateIndex(name: "IX_ProgressComment_UpdateId", table: "tbl_ProgressComments", column: "ProgressUpdateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tbl_ProgressComments");
            migrationBuilder.DropTable(name: "tbl_ProgressImages");
            migrationBuilder.DropTable(name: "tbl_ProgressUpdates");
            migrationBuilder.DropTable(name: "tbl_FundingEntries");
            migrationBuilder.DropTable(name: "tbl_ProjectSubscriptions");
            migrationBuilder.DropTable(name: "tbl_Stages");
            migrationBuilder.DropTable(name: "tbl_Projects_RS");
        }
    }
}
