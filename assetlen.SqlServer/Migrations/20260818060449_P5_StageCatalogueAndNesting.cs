using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class P5_StageCatalogueAndNesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CatalogueKey",
                table: "tbl_Stages",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentStageId",
                table: "tbl_Stages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phase",
                table: "tbl_Stages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Stages_ParentStageId",
                table: "tbl_Stages",
                column: "ParentStageId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Stages_tbl_Stages_ParentStageId",
                table: "tbl_Stages",
                column: "ParentStageId",
                principalTable: "tbl_Stages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Stages_tbl_Stages_ParentStageId",
                table: "tbl_Stages");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Stages_ParentStageId",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "CatalogueKey",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "ParentStageId",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "Phase",
                table: "tbl_Stages");
        }
    }
}
