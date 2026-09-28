using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P4_CommitmentsAndLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BaselineEndDate",
                table: "tbl_Stages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BaselineStartDate",
                table: "tbl_Stages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommitmentId",
                table: "tbl_Flags",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerMemberId",
                table: "tbl_Flags",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerPartyName",
                table: "tbl_Flags",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_Deliverables",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Deliverables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_AspNetUsers_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Deliverables_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_StageClaims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClaimedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenceArtifactId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClearedAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ClearedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClearedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    QueryNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_StageClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_AspNetUsers_ClaimedById",
                        column: x => x.ClaimedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_AspNetUsers_ClearedById",
                        column: x => x.ClearedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_StageClaims_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_Commitments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Maturity = table.Column<int>(type: "int", nullable: false),
                    QueryState = table.Column<int>(type: "int", nullable: false),
                    SourceChannel = table.Column<int>(type: "int", nullable: false),
                    AccountableMemberId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AgreedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AgreedWithMemberId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AgreedWithPartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AgreedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RecordedBySide = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    OwedBySide = table.Column<int>(type: "int", nullable: true),
                    SupersedesId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupersededById = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CounterpartyConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CounterpartyConfirmedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DisputedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisputedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DisputeNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ClearedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IngestedMessageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Commitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_AspNetUsers_AgreedById",
                        column: x => x.AgreedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_AspNetUsers_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Commitments_SupersedesId",
                        column: x => x.SupersedesId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_ProjectMembers_AccountableMemberId",
                        column: x => x.AccountableMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_ProjectMembers_AgreedWithMemberId",
                        column: x => x.AgreedWithMemberId,
                        principalTable: "tbl_ProjectMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Commitments_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_CommitmentLinks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Relation = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_CommitmentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentLinks_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_CommitmentLinks_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Variations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    StageId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CommitmentId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CostDelta = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TimeDeltaDays = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RaisedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RaisedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Access = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Variations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Variations_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_AspNetUsers_RaisedById",
                        column: x => x.RaisedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Commitments_CommitmentId",
                        column: x => x.CommitmentId,
                        principalTable: "tbl_Commitments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Variations_tbl_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "tbl_Stages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flag_CommitmentId",
                table: "tbl_Flags",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Flags_OwnerMemberId",
                table: "tbl_Flags",
                column: "OwnerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentLink_CommitmentId",
                table: "tbl_CommitmentLinks",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentLink_Target",
                table: "tbl_CommitmentLinks",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_DateTimeCreated",
                table: "tbl_CommitmentLinks",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_DateTimeModified",
                table: "tbl_CommitmentLinks",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_IsDeleted",
                table: "tbl_CommitmentLinks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_LastModifiedBy",
                table: "tbl_CommitmentLinks",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CommitmentLinks_ProjectId",
                table: "tbl_CommitmentLinks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "UX_CommitmentLink_Commitment_Target_Relation",
                table: "tbl_CommitmentLinks",
                columns: new[] { "CommitmentId", "TargetType", "TargetId", "Relation" },
                unique: true,
                filter: "[CommitmentId] IS NOT NULL AND [TargetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_DeliverableId",
                table: "tbl_Commitments",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_Project_Accountable",
                table: "tbl_Commitments",
                columns: new[] { "ProjectId", "AccountableMemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_ProjectId",
                table: "tbl_Commitments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_StageId",
                table: "tbl_Commitments",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitment_SupersedesId",
                table: "tbl_Commitments",
                column: "SupersedesId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AccountableMemberId",
                table: "tbl_Commitments",
                column: "AccountableMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AgreedById",
                table: "tbl_Commitments",
                column: "AgreedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_AgreedWithMemberId",
                table: "tbl_Commitments",
                column: "AgreedWithMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DateTimeCreated",
                table: "tbl_Commitments",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_DateTimeModified",
                table: "tbl_Commitments",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_IsDeleted",
                table: "tbl_Commitments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_LastModifiedBy",
                table: "tbl_Commitments",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Commitments_RecordedById",
                table: "tbl_Commitments",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverable_ProjectId",
                table: "tbl_Deliverables",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliverable_Stage_Order",
                table: "tbl_Deliverables",
                columns: new[] { "StageId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_CompletedById",
                table: "tbl_Deliverables",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_DateTimeCreated",
                table: "tbl_Deliverables",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_DateTimeModified",
                table: "tbl_Deliverables",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_IsDeleted",
                table: "tbl_Deliverables",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Deliverables_LastModifiedBy",
                table: "tbl_Deliverables",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StageClaim_ProjectId",
                table: "tbl_StageClaims",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StageClaim_StageId",
                table: "tbl_StageClaims",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_ClaimedById",
                table: "tbl_StageClaims",
                column: "ClaimedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_ClearedById",
                table: "tbl_StageClaims",
                column: "ClearedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_DateTimeCreated",
                table: "tbl_StageClaims",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_DateTimeModified",
                table: "tbl_StageClaims",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_IsDeleted",
                table: "tbl_StageClaims",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_StageClaims_LastModifiedBy",
                table: "tbl_StageClaims",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_ApprovedById",
                table: "tbl_Variations",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_CommitmentId",
                table: "tbl_Variations",
                column: "CommitmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_DateTimeCreated",
                table: "tbl_Variations",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_DateTimeModified",
                table: "tbl_Variations",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_IsDeleted",
                table: "tbl_Variations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_LastModifiedBy",
                table: "tbl_Variations",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Variations_RaisedById",
                table: "tbl_Variations",
                column: "RaisedById");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProjectId",
                table: "tbl_Variations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_StageId",
                table: "tbl_Variations",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Flags_tbl_Commitments_CommitmentId",
                table: "tbl_Flags",
                column: "CommitmentId",
                principalTable: "tbl_Commitments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Flags_tbl_ProjectMembers_OwnerMemberId",
                table: "tbl_Flags",
                column: "OwnerMemberId",
                principalTable: "tbl_ProjectMembers",
                principalColumn: "Id");

            // Stages dated before baselines existed take their current dates as
            // the baseline. The honest best available: any re-planning before
            // today is invisible, but every change from here on shows.
            migrationBuilder.Sql(
                "UPDATE tbl_Stages SET BaselineStartDate = StartDate, BaselineEndDate = ExpectedEndDate " +
                "WHERE BaselineStartDate IS NULL AND BaselineEndDate IS NULL " +
                "AND (StartDate IS NOT NULL OR ExpectedEndDate IS NOT NULL);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Flags_tbl_Commitments_CommitmentId",
                table: "tbl_Flags");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Flags_tbl_ProjectMembers_OwnerMemberId",
                table: "tbl_Flags");

            migrationBuilder.DropTable(
                name: "tbl_CommitmentLinks");

            migrationBuilder.DropTable(
                name: "tbl_StageClaims");

            migrationBuilder.DropTable(
                name: "tbl_Variations");

            migrationBuilder.DropTable(
                name: "tbl_Commitments");

            migrationBuilder.DropTable(
                name: "tbl_Deliverables");

            migrationBuilder.DropIndex(
                name: "IX_Flag_CommitmentId",
                table: "tbl_Flags");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Flags_OwnerMemberId",
                table: "tbl_Flags");

            migrationBuilder.DropColumn(
                name: "BaselineEndDate",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "BaselineStartDate",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "CommitmentId",
                table: "tbl_Flags");

            migrationBuilder.DropColumn(
                name: "OwnerMemberId",
                table: "tbl_Flags");

            migrationBuilder.DropColumn(
                name: "OwnerPartyName",
                table: "tbl_Flags");
        }
    }
}
