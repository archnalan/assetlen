using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P5_FundingSettlementAndMoneySeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HandlesMoney",
                table: "tbl_ProjectMembers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclaredAmount",
                table: "tbl_FundingEntries",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclaredCurrency",
                table: "tbl_FundingEntries",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceArtifactId",
                table: "tbl_FundingEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceFileName",
                table: "tbl_FundingEntries",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "tbl_FundingEntries",
                type: "decimal(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNote",
                table: "tbl_FundingEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedAmount",
                table: "tbl_FundingEntries",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                table: "tbl_FundingEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettledById",
                table: "tbl_FundingEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandlesMoney",
                table: "tbl_ProjectMembers");

            migrationBuilder.DropColumn(
                name: "DeclaredAmount",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "DeclaredCurrency",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "EvidenceArtifactId",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "EvidenceFileName",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "ReceiptNote",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "ReceivedAmount",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "SettledById",
                table: "tbl_FundingEntries");
        }
    }
}
