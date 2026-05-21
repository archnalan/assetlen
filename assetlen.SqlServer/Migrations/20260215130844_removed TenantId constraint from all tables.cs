using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace assetlen.Service.Migrations
{
    /// <inheritdoc />
    public partial class removedTenantIdconstraintfromalltables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_tbl_Tenants_TenantId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Banks_tbl_Tenants_TenantId",
                table: "tbl_Banks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_CashItems_tbl_Tenants_TenantId",
                table: "tbl_CashItems");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_category_tbl_Tenants_TenantId",
                table: "tbl_category");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_customerDeposit_tbl_Tenants_TenantId",
                table: "tbl_customerDeposit");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_customerPricing_tbl_Tenants_TenantId",
                table: "tbl_customerPricing");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Customers_tbl_Tenants_TenantId",
                table: "tbl_Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_discounts_tbl_Tenants_TenantId",
                table: "tbl_discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Expense_tbl_Tenants_TenantId",
                table: "tbl_Expense");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ExpenseType_tbl_Tenants_TenantId",
                table: "tbl_ExpenseType");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_FeedbackApprovals_tbl_Tenants_TenantId",
                table: "tbl_FeedbackApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_location_tbl_Tenants_TenantId",
                table: "tbl_location");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Logs_tbl_Tenants_TenantId",
                table: "tbl_Logs");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_OrderProcesses_tbl_Tenants_TenantId",
                table: "tbl_OrderProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_OrderStatus_tbl_Tenants_TenantId",
                table: "tbl_OrderStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_paymentAccounts_tbl_Tenants_TenantId",
                table: "tbl_paymentAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_paymentMode_tbl_Tenants_TenantId",
                table: "tbl_paymentMode");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Payments_tbl_Tenants_TenantId",
                table: "tbl_Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_PrinterPreferances_tbl_Tenants_TenantId",
                table: "tbl_PrinterPreferances");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProductDetailFeedbackReplies_tbl_Tenants_TenantId",
                table: "tbl_ProductDetailFeedbackReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProductDetailFeedbacks_tbl_Tenants_TenantId",
                table: "tbl_ProductDetailFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProductDetails_tbl_Tenants_TenantId",
                table: "tbl_ProductDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProductReceiving_tbl_Tenants_TenantId",
                table: "tbl_ProductReceiving");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ProductRelationships_tbl_Tenants_TenantId",
                table: "tbl_ProductRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_products_tbl_Tenants_TenantId",
                table: "tbl_products");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Refunds_tbl_Tenants_TenantId",
                table: "tbl_Refunds");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_RoleValues_tbl_Tenants_TenantId",
                table: "tbl_RoleValues");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_segment_tbl_Tenants_TenantId",
                table: "tbl_segment");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_ShiftClosureSummaries_tbl_Tenants_TenantId",
                table: "tbl_ShiftClosureSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_shifts_tbl_Tenants_TenantId",
                table: "tbl_shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Sizes_tbl_Tenants_TenantId",
                table: "tbl_Sizes");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_SlipLayout_tbl_Tenants_TenantId",
                table: "tbl_SlipLayout");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Supplier_tbl_Tenants_TenantId",
                table: "tbl_Supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_SupplierPayment_tbl_Tenants_TenantId",
                table: "tbl_SupplierPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_SyncLogs_tbl_Tenants_TenantId",
                table: "tbl_SyncLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_tax_tbl_Tenants_TenantId",
                table: "tbl_tax");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_transaction_tbl_Tenants_TenantId",
                table: "tbl_transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_transactionDetail_tbl_Tenants_TenantId",
                table: "tbl_transactionDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_UniqueFields_tbl_Tenants_TenantId",
                table: "tbl_UniqueFields");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_tbl_UniqueFields_TenantId",
                table: "tbl_UniqueFields");

            migrationBuilder.DropIndex(
                name: "IX_tbl_transactionDetail_TenantId",
                table: "tbl_transactionDetail");

            migrationBuilder.DropIndex(
                name: "IX_tbl_transaction_TenantId",
                table: "tbl_transaction");

            migrationBuilder.DropIndex(
                name: "IX_tbl_tax_TenantId",
                table: "tbl_tax");

            migrationBuilder.DropIndex(
                name: "IX_tbl_SyncLogs_TenantId",
                table: "tbl_SyncLogs");

            migrationBuilder.DropIndex(
                name: "IX_tbl_SupplierPayment_TenantId",
                table: "tbl_SupplierPayment");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Supplier_TenantId",
                table: "tbl_Supplier");

            migrationBuilder.DropIndex(
                name: "IX_tbl_SlipLayout_TenantId",
                table: "tbl_SlipLayout");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Sizes_TenantId",
                table: "tbl_Sizes");

            migrationBuilder.DropIndex(
                name: "IX_tbl_shifts_TenantId",
                table: "tbl_shifts");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ShiftClosureSummaries_TenantId",
                table: "tbl_ShiftClosureSummaries");

            migrationBuilder.DropIndex(
                name: "IX_tbl_segment_TenantId",
                table: "tbl_segment");

            migrationBuilder.DropIndex(
                name: "IX_tbl_RoleValues_TenantId",
                table: "tbl_RoleValues");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Refunds_TenantId",
                table: "tbl_Refunds");

            migrationBuilder.DropIndex(
                name: "IX_tbl_products_TenantId",
                table: "tbl_products");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProductRelationships_TenantId",
                table: "tbl_ProductRelationships");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProductReceiving_TenantId",
                table: "tbl_ProductReceiving");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProductDetails_TenantId",
                table: "tbl_ProductDetails");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProductDetailFeedbacks_TenantId",
                table: "tbl_ProductDetailFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_TenantId",
                table: "tbl_ProductDetailFeedbackReplies");

            migrationBuilder.DropIndex(
                name: "IX_tbl_PrinterPreferances_TenantId",
                table: "tbl_PrinterPreferances");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Payments_TenantId",
                table: "tbl_Payments");

            migrationBuilder.DropIndex(
                name: "IX_tbl_paymentMode_TenantId",
                table: "tbl_paymentMode");

            migrationBuilder.DropIndex(
                name: "IX_tbl_paymentAccounts_TenantId",
                table: "tbl_paymentAccounts");

            migrationBuilder.DropIndex(
                name: "IX_tbl_OrderStatus_TenantId",
                table: "tbl_OrderStatus");

            migrationBuilder.DropIndex(
                name: "IX_tbl_OrderProcesses_TenantId",
                table: "tbl_OrderProcesses");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Logs_TenantId",
                table: "tbl_Logs");

            migrationBuilder.DropIndex(
                name: "IX_tbl_location_TenantId",
                table: "tbl_location");

            migrationBuilder.DropIndex(
                name: "IX_tbl_FeedbackApprovals_TenantId",
                table: "tbl_FeedbackApprovals");

            migrationBuilder.DropIndex(
                name: "IX_tbl_ExpenseType_TenantId",
                table: "tbl_ExpenseType");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Expense_TenantId",
                table: "tbl_Expense");

            migrationBuilder.DropIndex(
                name: "IX_tbl_discounts_TenantId",
                table: "tbl_discounts");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Customers_TenantId",
                table: "tbl_Customers");

            migrationBuilder.DropIndex(
                name: "IX_tbl_customerPricing_TenantId",
                table: "tbl_customerPricing");

            migrationBuilder.DropIndex(
                name: "IX_tbl_customerDeposit_TenantId",
                table: "tbl_customerDeposit");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Configuration_TenantId1",
                table: "tbl_Configuration");

            migrationBuilder.DropIndex(
                name: "IX_tbl_category_TenantId",
                table: "tbl_category");

            migrationBuilder.DropIndex(
                name: "IX_tbl_CashItems_TenantId",
                table: "tbl_CashItems");

            migrationBuilder.DropIndex(
                name: "IX_tbl_Banks_TenantId",
                table: "tbl_Banks");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TenantId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_UniqueFields",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_transactionDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_tax",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SyncLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SupplierPayment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Supplier",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SlipLayout",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Sizes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_shifts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ShiftClosureSummaries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_segment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_RoleValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Refunds",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductRelationships",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductReceiving",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetailFeedbacks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetailFeedbackReplies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_PrinterPreferances",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_paymentMode",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_paymentAccounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_OrderStatus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_OrderProcesses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Logs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_location",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_FeedbackApprovals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ExpenseType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Expense",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_discounts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Customers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_customerPricing",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_customerDeposit",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_category",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_CashItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Banks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration");

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_UniqueFields",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_transactionDetail",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_transaction",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_tax",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SyncLogs",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SupplierPayment",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Supplier",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_SlipLayout",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Sizes",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_shifts",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ShiftClosureSummaries",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_segment",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_RoleValues",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Refunds",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_products",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductRelationships",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductReceiving",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetails",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetailFeedbacks",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ProductDetailFeedbackReplies",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_PrinterPreferances",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Payments",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_paymentMode",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_paymentAccounts",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_OrderStatus",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_OrderProcesses",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Logs",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_location",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_FeedbackApprovals",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_ExpenseType",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Expense",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_discounts",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Customers",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_customerPricing",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_customerDeposit",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId1",
                table: "tbl_Configuration",
                type: "nvarchar(36)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_category",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_CashItems",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "tbl_Banks",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantId",
                table: "RefreshTokens",
                type: "nvarchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_UniqueFields_TenantId",
                table: "tbl_UniqueFields",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transactionDetail_TenantId",
                table: "tbl_transactionDetail",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_TenantId",
                table: "tbl_transaction",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tax_TenantId",
                table: "tbl_tax",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SyncLogs_TenantId",
                table: "tbl_SyncLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SupplierPayment_TenantId",
                table: "tbl_SupplierPayment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Supplier_TenantId",
                table: "tbl_Supplier",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SlipLayout_TenantId",
                table: "tbl_SlipLayout",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Sizes_TenantId",
                table: "tbl_Sizes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_shifts_TenantId",
                table: "tbl_shifts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ShiftClosureSummaries_TenantId",
                table: "tbl_ShiftClosureSummaries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_segment_TenantId",
                table: "tbl_segment",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_RoleValues_TenantId",
                table: "tbl_RoleValues",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Refunds_TenantId",
                table: "tbl_Refunds",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_products_TenantId",
                table: "tbl_products",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductRelationships_TenantId",
                table: "tbl_ProductRelationships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductReceiving_TenantId",
                table: "tbl_ProductReceiving",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetails_TenantId",
                table: "tbl_ProductDetails",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbacks_TenantId",
                table: "tbl_ProductDetailFeedbacks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ProductDetailFeedbackReplies_TenantId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_PrinterPreferances_TenantId",
                table: "tbl_PrinterPreferances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Payments_TenantId",
                table: "tbl_Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentMode_TenantId",
                table: "tbl_paymentMode",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_paymentAccounts_TenantId",
                table: "tbl_paymentAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderStatus_TenantId",
                table: "tbl_OrderStatus",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_OrderProcesses_TenantId",
                table: "tbl_OrderProcesses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Logs_TenantId",
                table: "tbl_Logs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_location_TenantId",
                table: "tbl_location",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_FeedbackApprovals_TenantId",
                table: "tbl_FeedbackApprovals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ExpenseType_TenantId",
                table: "tbl_ExpenseType",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Expense_TenantId",
                table: "tbl_Expense",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_discounts_TenantId",
                table: "tbl_discounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Customers_TenantId",
                table: "tbl_Customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerPricing_TenantId",
                table: "tbl_customerPricing",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_customerDeposit_TenantId",
                table: "tbl_customerDeposit",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Configuration_TenantId1",
                table: "tbl_Configuration",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_category_TenantId",
                table: "tbl_category",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_CashItems_TenantId",
                table: "tbl_CashItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Banks_TenantId",
                table: "tbl_Banks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TenantId",
                table: "RefreshTokens",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_tbl_Tenants_TenantId",
                table: "RefreshTokens",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Banks_tbl_Tenants_TenantId",
                table: "tbl_Banks",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_CashItems_tbl_Tenants_TenantId",
                table: "tbl_CashItems",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_category_tbl_Tenants_TenantId",
                table: "tbl_category",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId",
                table: "tbl_Configuration",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Configuration_tbl_Tenants_TenantId1",
                table: "tbl_Configuration",
                column: "TenantId1",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_customerDeposit_tbl_Tenants_TenantId",
                table: "tbl_customerDeposit",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_customerPricing_tbl_Tenants_TenantId",
                table: "tbl_customerPricing",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Customers_tbl_Tenants_TenantId",
                table: "tbl_Customers",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_discounts_tbl_Tenants_TenantId",
                table: "tbl_discounts",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Expense_tbl_Tenants_TenantId",
                table: "tbl_Expense",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ExpenseType_tbl_Tenants_TenantId",
                table: "tbl_ExpenseType",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_FeedbackApprovals_tbl_Tenants_TenantId",
                table: "tbl_FeedbackApprovals",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_location_tbl_Tenants_TenantId",
                table: "tbl_location",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Logs_tbl_Tenants_TenantId",
                table: "tbl_Logs",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_OrderProcesses_tbl_Tenants_TenantId",
                table: "tbl_OrderProcesses",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_OrderStatus_tbl_Tenants_TenantId",
                table: "tbl_OrderStatus",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_paymentAccounts_tbl_Tenants_TenantId",
                table: "tbl_paymentAccounts",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_paymentMode_tbl_Tenants_TenantId",
                table: "tbl_paymentMode",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Payments_tbl_Tenants_TenantId",
                table: "tbl_Payments",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_PrinterPreferances_tbl_Tenants_TenantId",
                table: "tbl_PrinterPreferances",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProductDetailFeedbackReplies_tbl_Tenants_TenantId",
                table: "tbl_ProductDetailFeedbackReplies",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProductDetailFeedbacks_tbl_Tenants_TenantId",
                table: "tbl_ProductDetailFeedbacks",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProductDetails_tbl_Tenants_TenantId",
                table: "tbl_ProductDetails",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProductReceiving_tbl_Tenants_TenantId",
                table: "tbl_ProductReceiving",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ProductRelationships_tbl_Tenants_TenantId",
                table: "tbl_ProductRelationships",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_products_tbl_Tenants_TenantId",
                table: "tbl_products",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Refunds_tbl_Tenants_TenantId",
                table: "tbl_Refunds",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_RoleValues_tbl_Tenants_TenantId",
                table: "tbl_RoleValues",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_segment_tbl_Tenants_TenantId",
                table: "tbl_segment",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_ShiftClosureSummaries_tbl_Tenants_TenantId",
                table: "tbl_ShiftClosureSummaries",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_shifts_tbl_Tenants_TenantId",
                table: "tbl_shifts",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Sizes_tbl_Tenants_TenantId",
                table: "tbl_Sizes",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_SlipLayout_tbl_Tenants_TenantId",
                table: "tbl_SlipLayout",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_Supplier_tbl_Tenants_TenantId",
                table: "tbl_Supplier",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_SupplierPayment_tbl_Tenants_TenantId",
                table: "tbl_SupplierPayment",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_SyncLogs_tbl_Tenants_TenantId",
                table: "tbl_SyncLogs",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_tax_tbl_Tenants_TenantId",
                table: "tbl_tax",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_transaction_tbl_Tenants_TenantId",
                table: "tbl_transaction",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_transactionDetail_tbl_Tenants_TenantId",
                table: "tbl_transactionDetail",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_UniqueFields_tbl_Tenants_TenantId",
                table: "tbl_UniqueFields",
                column: "TenantId",
                principalTable: "tbl_Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
