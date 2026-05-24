using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRepayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "repayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoanId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Reference = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repayments", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_CustomerId",
                table: "repayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_LoanId",
                table: "repayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_PaymentDate",
                table: "repayments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_RecordedByUserId",
                table: "repayments",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_repayments_Reference",
                table: "repayments",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repayments");
        }
    }
}
