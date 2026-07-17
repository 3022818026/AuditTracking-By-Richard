using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditTracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRectificationVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RectificationVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditIssueId = table.Column<int>(type: "int", nullable: false),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    VerificationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VerificationResult = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VerificationComment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Verifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RectificationVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RectificationVerifications_AuditIssues_AuditIssueId",
                        column: x => x.AuditIssueId,
                        principalTable: "AuditIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RectificationVerifications_CorrectiveActions_CorrectiveActionId",
                        column: x => x.CorrectiveActionId,
                        principalTable: "CorrectiveActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerifications_AuditIssueId",
                table: "RectificationVerifications",
                column: "AuditIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerifications_CorrectiveActionId",
                table: "RectificationVerifications",
                column: "CorrectiveActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerifications_VerificationNo",
                table: "RectificationVerifications",
                column: "VerificationNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerifications_VerificationResult",
                table: "RectificationVerifications",
                column: "VerificationResult");

            migrationBuilder.CreateIndex(
                name: "IX_RectificationVerifications_VerifiedAt",
                table: "RectificationVerifications",
                column: "VerifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RectificationVerifications");
        }
    }
}
