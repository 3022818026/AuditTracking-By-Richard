using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditTracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectiveActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrectiveActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditIssueId = table.Column<int>(type: "int", nullable: false),
                    ActionNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ResponsibleDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlannedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CorrectiveActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_AuditIssues_AuditIssueId",
                        column: x => x.AuditIssueId,
                        principalTable: "AuditIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_ActionNo",
                table: "CorrectiveActions",
                column: "ActionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_AuditIssueId",
                table: "CorrectiveActions",
                column: "AuditIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_PlannedCompletionDate",
                table: "CorrectiveActions",
                column: "PlannedCompletionDate");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_Status",
                table: "CorrectiveActions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectiveActions");
        }
    }
}
