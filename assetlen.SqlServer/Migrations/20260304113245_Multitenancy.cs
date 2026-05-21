using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class Multitenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_UserFavorites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_UserDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_UniqueFields",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_transactionDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_tax",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_SyncLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_SupplierPayment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Supplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_SubscriptionSeats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_SubscriptionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StageName",
                table: "tbl_Stages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_Stages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionPercentage",
                table: "tbl_Stages",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BudgetAmount",
                table: "tbl_Stages",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Stages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_SlipLayout",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Sizes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ShiftClosureSummaries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_segment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_RoleValues",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_ProjectSubscriptions",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "InvestorId",
                table: "tbl_ProjectSubscriptions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProjectSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBudget",
                table: "tbl_Projects_RS",
                type: "decimal(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectName",
                table: "tbl_Projects_RS",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "InvestorId",
                table: "tbl_Projects_RS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Projects_RS",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StageId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionPercentage",
                table: "tbl_ProgressUpdates",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProgressUpdates",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProgressUpdateId",
                table: "tbl_ProgressImages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "tbl_ProgressImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProgressImages",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentText",
                table: "tbl_ProgressComments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "tbl_ProgressComments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProgressComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProductRelationships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProductReceiving",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProductDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProductDetailFeedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ProductDetailFeedbackReplies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_PrinterPreferances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_paymentMode",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_paymentAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_OrderStatus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_OrderProcesses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Logs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_location",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StageId",
                table: "tbl_FundingEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_FundingEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "tbl_FundingEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "PaidById",
                table: "tbl_FundingEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_FundingEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_FeedbackApprovals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_ExpenseType",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Expense",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_EmployeeApprovals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_discounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_customerPricing",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Configuration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_category",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_CashItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "tbl_Banks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Access",
                table: "RefreshTokens",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Access",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_UserFavorites");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_UserDocuments");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_UniqueFields");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_transactionDetail");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_transaction");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_tax");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_SyncLogs");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_SupplierPayment");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Supplier");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_SubscriptionSeats");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_SubscriptionRequests");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Stages");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_SlipLayout");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Sizes");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_shifts");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ShiftClosureSummaries");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_segment");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_RoleValues");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProjectSubscriptions");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Projects_RS");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProgressUpdates");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProgressImages");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProgressComments");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_products");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProductRelationships");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProductReceiving");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProductDetails");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProductDetailFeedbacks");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ProductDetailFeedbackReplies");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_PrinterPreferances");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Payments");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_paymentMode");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_paymentAccounts");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_OrderStatus");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_OrderProcesses");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Logs");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_location");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_FundingEntries");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_FeedbackApprovals");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_ExpenseType");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Expense");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_EmployeeApprovals");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_discounts");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Customers");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_customerPricing");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Configuration");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_category");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_CashItems");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "tbl_Banks");

            migrationBuilder.DropColumn(
                name: "Access",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "StageName",
                table: "tbl_Stages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_Stages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionPercentage",
                table: "tbl_Stages",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BudgetAmount",
                table: "tbl_Stages",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_ProjectSubscriptions",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvestorId",
                table: "tbl_ProjectSubscriptions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBudget",
                table: "tbl_Projects_RS",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectName",
                table: "tbl_Projects_RS",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvestorId",
                table: "tbl_Projects_RS",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StageId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "tbl_ProgressUpdates",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CompletionPercentage",
                table: "tbl_ProgressUpdates",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProgressUpdateId",
                table: "tbl_ProgressImages",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "tbl_ProgressImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommentText",
                table: "tbl_ProgressComments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorId",
                table: "tbl_ProgressComments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StageId",
                table: "tbl_FundingEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProjectId",
                table: "tbl_FundingEntries",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "tbl_FundingEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaidById",
                table: "tbl_FundingEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
