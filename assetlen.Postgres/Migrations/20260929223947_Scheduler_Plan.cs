using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Scheduler_Plan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStart",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CureDays",
                table: "tbl_Deliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EarliestStart",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MakeDays",
                table: "tbl_Deliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinnedFinish",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedMakeEnd",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedMakeStart",
                table: "tbl_Deliverables",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueOrder",
                table: "tbl_Deliverables",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamKey",
                table: "tbl_Deliverables",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tbl_PlanWaits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    DeliverableId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Arrival = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PredecessorId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Link = table.Column<int>(type: "integer", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false),
                    CalendarDays = table.Column<bool>(type: "boolean", nullable: false),
                    FromDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UntilDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AfterMaking = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    ClearedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClearedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemovedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PlanWaits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_PlanWaits_tbl_Deliverables_DeliverableId",
                        column: x => x.DeliverableId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_PlanWaits_tbl_Deliverables_PredecessorId",
                        column: x => x.PredecessorId,
                        principalTable: "tbl_Deliverables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_WorkSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProjectId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    RestDay = table.Column<int>(type: "integer", nullable: false),
                    Holidays = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExtendLateWaits = table.Column<bool>(type: "boolean", nullable: false),
                    LastComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSavedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastSavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorksComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousWorksComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorksCompleteMovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReserveDays = table.Column<int>(type: "integer", nullable: true),
                    CriticalPath = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_WorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_WorkSchedules_tbl_Projects_RS_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "tbl_Projects_RS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanWait_DeliverableId",
                table: "tbl_PlanWaits",
                column: "DeliverableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanWait_ProjectId",
                table: "tbl_PlanWaits",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PlanWaits_DateTimeCreated",
                table: "tbl_PlanWaits",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PlanWaits_DateTimeModified",
                table: "tbl_PlanWaits",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PlanWaits_IsDeleted",
                table: "tbl_PlanWaits",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PlanWaits_LastModifiedBy",
                table: "tbl_PlanWaits",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PlanWaits_PredecessorId",
                table: "tbl_PlanWaits",
                column: "PredecessorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorkSchedules_DateTimeCreated",
                table: "tbl_WorkSchedules",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorkSchedules_DateTimeModified",
                table: "tbl_WorkSchedules",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorkSchedules_IsDeleted",
                table: "tbl_WorkSchedules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_WorkSchedules_LastModifiedBy",
                table: "tbl_WorkSchedules",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedule_ProjectId",
                table: "tbl_WorkSchedules",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_PlanWaits");

            migrationBuilder.DropTable(
                name: "tbl_WorkSchedules");

            migrationBuilder.DropColumn(
                name: "ActualStart",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "CureDays",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "EarliestStart",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "MakeDays",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "PinnedFinish",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "PlannedMakeEnd",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "PlannedMakeStart",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "QueueOrder",
                table: "tbl_Deliverables");

            migrationBuilder.DropColumn(
                name: "TeamKey",
                table: "tbl_Deliverables");
        }
    }
}
