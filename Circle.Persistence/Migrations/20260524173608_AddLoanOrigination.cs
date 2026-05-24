using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanOrigination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoanCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestRatePercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalRepayable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TermCount = table.Column<int>(type: "int", nullable: false),
                    RepaymentFrequency = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    InterestCalculationMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MaturityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ApprovalNote = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Purpose = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "loan_repayment_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_repayment_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loan_repayment_schedules_loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_loan_repayment_schedules_DueDate",
                table: "loan_repayment_schedules",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_loan_repayment_schedules_LoanId",
                table: "loan_repayment_schedules",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_loan_repayment_schedules_LoanId_InstallmentNumber",
                table: "loan_repayment_schedules",
                columns: new[] { "LoanId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loans_ApprovedByUserId",
                table: "loans",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_loans_CustomerId",
                table: "loans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_loans_LoanCode",
                table: "loans",
                column: "LoanCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loans_RequestedByUserId",
                table: "loans",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_loans_Status",
                table: "loans",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_repayment_schedules");

            migrationBuilder.DropTable(
                name: "loans");
        }
    }
}
