using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerVirtualAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_virtual_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    AccountName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    BankName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    BankCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    ProviderReference = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_virtual_accounts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_customer_virtual_accounts_AccountNumber",
                table: "customer_virtual_accounts",
                column: "AccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_customer_virtual_accounts_CustomerId",
                table: "customer_virtual_accounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_virtual_accounts_ProviderReference",
                table: "customer_virtual_accounts",
                column: "ProviderReference");

            migrationBuilder.CreateIndex(
                name: "IX_customer_virtual_accounts_Status",
                table: "customer_virtual_accounts",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_virtual_accounts");
        }
    }
}
