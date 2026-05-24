using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanDisbursements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loan_disbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DisbursementDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Reference = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    DisbursedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_disbursements", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_CustomerId",
                table: "loan_disbursements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_DisbursedByUserId",
                table: "loan_disbursements",
                column: "DisbursedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_DisbursementDate",
                table: "loan_disbursements",
                column: "DisbursementDate");

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_LoanId",
                table: "loan_disbursements",
                column: "LoanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loan_disbursements_Reference",
                table: "loan_disbursements",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loan_disbursements");
        }
    }
}
