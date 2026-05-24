using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundPaymentReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbound_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ProviderReference = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnappliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PayerName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    Narration = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: true),
                    LoanId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RepaymentId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FailureReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_payments", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_AccountNumber",
                table: "inbound_payments",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_CustomerId",
                table: "inbound_payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_LoanId",
                table: "inbound_payments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_PaidAtUtc",
                table: "inbound_payments",
                column: "PaidAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_ProviderReference",
                table: "inbound_payments",
                column: "ProviderReference");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_RepaymentId",
                table: "inbound_payments",
                column: "RepaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_payments_Status",
                table: "inbound_payments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbound_payments");
        }
    }
}
