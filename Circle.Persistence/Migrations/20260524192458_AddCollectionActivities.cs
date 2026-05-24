using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collection_activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LoanId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RepaymentScheduleId = table.Column<Guid>(type: "char(36)", nullable: true),
                    OfficerUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ActivityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Outcome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PromisedPaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PromisedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection_activities", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_CreatedAtUtc",
                table: "collection_activities",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_CustomerId",
                table: "collection_activities",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_LoanId",
                table: "collection_activities",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_OfficerUserId",
                table: "collection_activities",
                column: "OfficerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_PromisedPaymentDate",
                table: "collection_activities",
                column: "PromisedPaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_collection_activities_RepaymentScheduleId",
                table: "collection_activities",
                column: "RepaymentScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_activities");
        }
    }
}
