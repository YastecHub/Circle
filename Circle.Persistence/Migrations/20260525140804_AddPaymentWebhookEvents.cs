using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "RecordedByUserId",
                table: "repayments",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.CreateTable(
                name: "payment_webhook_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Provider = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    EventType = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    EventReference = table.Column<string>(type: "varchar(180)", maxLength: 180, nullable: true),
                    PayloadJson = table.Column<string>(type: "longtext", nullable: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    InboundPaymentId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ProcessingMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_webhook_events", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_payment_webhook_events_CreatedAtUtc",
                table: "payment_webhook_events",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_payment_webhook_events_EventReference",
                table: "payment_webhook_events",
                column: "EventReference");

            migrationBuilder.CreateIndex(
                name: "IX_payment_webhook_events_InboundPaymentId",
                table: "payment_webhook_events",
                column: "InboundPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_webhook_events_Provider",
                table: "payment_webhook_events",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_payment_webhook_events_Status",
                table: "payment_webhook_events",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_webhook_events");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecordedByUserId",
                table: "repayments",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }
    }
}
