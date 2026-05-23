using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Circle.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Action = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    AffectedRecord = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    IpAddress = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    PassbookReference = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    FullName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Phone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    AlternatePhone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    Address = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    GovernmentIdType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    GovernmentIdNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    Occupation = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    GuarantorName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true),
                    GuarantorPhone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true),
                    GuarantorRelationship = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    GuarantorIdNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    AssignedCreditOfficerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Key = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Value = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    FullName = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Phone = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Branch = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DocumentType = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_documents_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Key", "UpdatedAtUtc", "Value" },
                values: new object[,]
                {
                    { new Guid("31ba8b6d-b37a-4dc1-8247-6f1bc8f8e7b1"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Selected MVP payment gateway.", "PaymentGateway", null, "Monnify" },
                    { new Guid("bed10e4a-9bb1-4939-b8e0-a6c432e4f37d"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Loans above this amount require Business Owner sign-off.", "AutoApprovalThreshold", null, "20000" },
                    { new Guid("c9d69918-1202-42f8-9b61-116f56f36d2b"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Repayment frequencies supported in MVP.", "RepaymentFrequencies", null, "Daily,Weekly" },
                    { new Guid("f0ce80ac-6d1d-4e5e-8b4e-2f3f0a57de68"), new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Selected MVP interest calculation method.", "InterestCalculationMethod", null, "FlatRate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorUserId",
                table: "audit_logs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAtUtc",
                table: "audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_customer_documents_CustomerId",
                table: "customer_documents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_documents_UploadedByUserId",
                table: "customer_documents",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_AssignedCreditOfficerId",
                table: "customers",
                column: "AssignedCreditOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_CustomerCode",
                table: "customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_Key",
                table: "system_settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "customer_documents");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
