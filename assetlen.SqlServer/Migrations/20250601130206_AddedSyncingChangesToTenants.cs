using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddedSyncingChangesToTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CocurrencyKey",
                table: "tbl_Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tbl_Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRenewal",
                table: "tbl_Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "keyHarsh",
                table: "tbl_Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CocurrencyKey",
                table: "tbl_Tenants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "tbl_Tenants");

            migrationBuilder.DropColumn(
                name: "LastRenewal",
                table: "tbl_Tenants");

            migrationBuilder.DropColumn(
                name: "keyHarsh",
                table: "tbl_Tenants");
        }
    }
}
