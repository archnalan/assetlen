using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mowt.Service.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_SubscriptionRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OrganisationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RequestedSeats = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SubmittedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubmittedByEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuotedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuoteCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    QuoteNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuotedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuotedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    QuotedByUserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentConfirmedByUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SubscriptionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SubscriptionSeats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActivatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LinkedUserId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTimeModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SubscriptionSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SubscriptionSeats_tbl_SubscriptionRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "tbl_SubscriptionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_DateTimeCreated",
                table: "tbl_SubscriptionRequests",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_DateTimeModified",
                table: "tbl_SubscriptionRequests",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_IsDeleted",
                table: "tbl_SubscriptionRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionRequests_LastModifiedBy",
                table: "tbl_SubscriptionRequests",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_DateTimeCreated",
                table: "tbl_SubscriptionSeats",
                column: "DateTimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_DateTimeModified",
                table: "tbl_SubscriptionSeats",
                column: "DateTimeModified");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_IsDeleted",
                table: "tbl_SubscriptionSeats",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_LastModifiedBy",
                table: "tbl_SubscriptionSeats",
                column: "LastModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SubscriptionSeats_RequestId_Email",
                table: "tbl_SubscriptionSeats",
                columns: new[] { "RequestId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_SubscriptionSeats");

            migrationBuilder.DropTable(
                name: "tbl_SubscriptionRequests");
        }
    }
}
