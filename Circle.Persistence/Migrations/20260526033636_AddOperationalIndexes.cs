using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_repayments_CustomerId_PaymentDate",
                table: "repayments",
                columns: new[] { "CustomerId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_loans_Status_CreatedAtUtc",
                table: "loans",
                columns: new[] { "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_loan_repayment_schedules_Status_DueDate",
                table: "loan_repayment_schedules",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_CustomerId_DisbursementDate",
                table: "loan_disbursements",
                columns: new[] { "CustomerId", "DisbursementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_Provider_ProviderReference",
                table: "inbound_payments",
                columns: new[] { "Provider", "ProviderReference" });

            migrationBuilder.CreateIndex(
                name: "IX_customer_virtual_accounts_Provider_AccountNumber_Status",
                table: "customer_virtual_accounts",
                columns: new[] { "Provider", "AccountNumber", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_Outcome_PromisedPaymentDate",
                table: "collection_activities",
                columns: new[] { "Outcome", "PromisedPaymentDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_repayments_CustomerId_PaymentDate",
                table: "repayments");

            migrationBuilder.DropIndex(
                name: "IX_loans_Status_CreatedAtUtc",
                table: "loans");

            migrationBuilder.DropIndex(
                name: "IX_loan_repayment_schedules_Status_DueDate",
                table: "loan_repayment_schedules");

            migrationBuilder.DropIndex(
                name: "IX_loan_disbursements_CustomerId_DisbursementDate",
                table: "loan_disbursements");

            migrationBuilder.DropIndex(
                name: "IX_inbound_payments_Provider_ProviderReference",
                table: "inbound_payments");

            migrationBuilder.DropIndex(
                name: "IX_customer_virtual_accounts_Provider_AccountNumber_Status",
                table: "customer_virtual_accounts");

            migrationBuilder.DropIndex(
                name: "IX_collection_activities_Outcome_PromisedPaymentDate",
                table: "collection_activities");
        }
    }
}
