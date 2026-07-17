using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditTracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectiveActionOperationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrectiveActionOperationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    ActionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BeforeData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActionOperationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionOperationLogs_CorrectiveActionId",
                table: "CorrectiveActionOperationLogs",
                column: "CorrectiveActionId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionOperationLogs_CreatedAt",
                table: "CorrectiveActionOperationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionOperationLogs_OperationType",
                table: "CorrectiveActionOperationLogs",
                column: "OperationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectiveActionOperationLogs");
        }
    }
}
