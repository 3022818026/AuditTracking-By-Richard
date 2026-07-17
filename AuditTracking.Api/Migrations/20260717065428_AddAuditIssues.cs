using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditTracking.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditPlanId = table.Column<int>(type: "int", nullable: false),
                    IssueNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IssueType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ResponsibleDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AuditIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditIssues_AuditPlans_AuditPlanId",
                        column: x => x.AuditPlanId,
                        principalTable: "AuditPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditIssues_AuditPlanId",
                table: "AuditIssues",
                column: "AuditPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditIssues_DueDate",
                table: "AuditIssues",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditIssues_IssueNo",
                table: "AuditIssues",
                column: "IssueNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditIssues_Status",
                table: "AuditIssues",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditIssues");
        }
    }
}
