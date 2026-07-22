using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditTracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRectificationVerificationOperationLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RectificationVerificationOperationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RectificationVerificationId = table.Column<int>(type: "int", nullable: false),
                    VerificationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BeforeData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RectificationVerificationOperationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerificationOperationLogs_CreatedAt",
                table: "RectificationVerificationOperationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerificationOperationLogs_OperationType",
                table: "RectificationVerificationOperationLogs",
                column: "OperationType");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerificationOperationLogs_RectificationVerificationId",
                table: "RectificationVerificationOperationLogs",
                column: "RectificationVerificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RectificationVerificationOperationLogs");
        }
    }
}
