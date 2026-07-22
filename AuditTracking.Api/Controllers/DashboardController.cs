using AuditTracking.Api.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext dbContext,
        ILogger<DashboardController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var today = DateTime.Today;

        var auditPlanStats = await _dbContext.AuditPlans
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Draft = group.Count(x => x.Status == "Draft"),
                InProgress = group.Count(x => x.Status == "InProgress"),
                Completed = group.Count(x => x.Status == "Completed"),
                Closed = group.Count(x => x.Status == "Closed"),
                Cancelled = group.Count(x => x.Status == "Cancelled"),
                Overdue = group.Count(x =>
                    x.PlannedDate < today &&
                    x.Status != "Completed" &&
                    x.Status != "Closed" &&
                    x.Status != "Cancelled")
            })
            .FirstOrDefaultAsync();

        var auditIssueStats = await _dbContext.AuditIssues
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Open = group.Count(x => x.Status == "Open"),
                Rectifying = group.Count(x => x.Status == "Rectifying"),
                PendingVerification = group.Count(x => x.Status == "PendingVerification"),
                Closed = group.Count(x => x.Status == "Closed"),
                Rejected = group.Count(x => x.Status == "Rejected"),
                Overdue = group.Count(x =>
                    x.DueDate.HasValue &&
                    x.DueDate.Value < today &&
                    x.Status != "Closed" &&
                    x.Status != "Rejected")
            })
            .FirstOrDefaultAsync();

        var correctiveActionStats = await _dbContext.CorrectiveActions
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Draft = group.Count(x => x.Status == "Draft"),
                Submitted = group.Count(x => x.Status == "Submitted"),
                Approved = group.Count(x => x.Status == "Approved"),
                Rejected = group.Count(x => x.Status == "Rejected"),
                Completed = group.Count(x => x.Status == "Completed"),
                Overdue = group.Count(x =>
                    x.PlannedCompletionDate.HasValue &&
                    x.PlannedCompletionDate.Value < today &&
                    x.Status != "Completed")
            })
            .FirstOrDefaultAsync();

        var verificationStats = await _dbContext.RectificationVerifications
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Passed = group.Count(x => x.VerificationResult == "Passed"),
                Failed = group.Count(x => x.VerificationResult == "Failed"),
                NeedMoreEvidence = group.Count(x => x.VerificationResult == "NeedMoreEvidence")
            })
            .FirstOrDefaultAsync();

        var recentAuditPlans = await _dbContext.AuditPlans
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new RecentAuditPlanDto
            {
                Id = x.Id,
                AuditNo = x.AuditNo,
                Title = x.Title,
                Status = x.Status,
                PlannedDate = x.PlannedDate,
                Auditee = x.Auditee,
                Auditor = x.Auditor,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var recentAuditIssues = await _dbContext.AuditIssues
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new RecentAuditIssueDto
            {
                Id = x.Id,
                IssueNo = x.IssueNo,
                Title = x.Title,
                Severity = x.Severity,
                Status = x.Status,
                DueDate = x.DueDate,
                ResponsibleDepartment = x.ResponsibleDepartment,
                ResponsiblePerson = x.ResponsiblePerson,
                AuditPlanId = x.AuditPlanId,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var correctiveActionTotal = correctiveActionStats?.Total ?? 0;
        var correctiveActionCompleted = correctiveActionStats?.Completed ?? 0;
        var correctiveActionCompletionRate = correctiveActionTotal == 0
            ? 0m
            : Math.Round(
                correctiveActionCompleted * 100m / correctiveActionTotal,
                2,
                MidpointRounding.AwayFromZero);

        var result = new DashboardSummaryDto
        {
            AuditPlanTotal = auditPlanStats?.Total ?? 0,
            AuditPlanDraft = auditPlanStats?.Draft ?? 0,
            AuditPlanInProgress = auditPlanStats?.InProgress ?? 0,
            AuditPlanCompleted = auditPlanStats?.Completed ?? 0,
            AuditPlanClosed = auditPlanStats?.Closed ?? 0,
            AuditPlanCancelled = auditPlanStats?.Cancelled ?? 0,
            AuditPlanOverdue = auditPlanStats?.Overdue ?? 0,

            AuditIssueTotal = auditIssueStats?.Total ?? 0,
            AuditIssueOpen = auditIssueStats?.Open ?? 0,
            AuditIssueRectifying = auditIssueStats?.Rectifying ?? 0,
            AuditIssuePendingVerification = auditIssueStats?.PendingVerification ?? 0,
            AuditIssueClosed = auditIssueStats?.Closed ?? 0,
            AuditIssueRejected = auditIssueStats?.Rejected ?? 0,
            AuditIssueOverdue = auditIssueStats?.Overdue ?? 0,

            CorrectiveActionTotal = correctiveActionTotal,
            CorrectiveActionDraft = correctiveActionStats?.Draft ?? 0,
            CorrectiveActionSubmitted = correctiveActionStats?.Submitted ?? 0,
            CorrectiveActionApproved = correctiveActionStats?.Approved ?? 0,
            CorrectiveActionRejected = correctiveActionStats?.Rejected ?? 0,
            CorrectiveActionCompleted = correctiveActionCompleted,
            CorrectiveActionOverdue = correctiveActionStats?.Overdue ?? 0,

            RectificationVerificationTotal = verificationStats?.Total ?? 0,
            RectificationVerificationPassed = verificationStats?.Passed ?? 0,
            RectificationVerificationFailed = verificationStats?.Failed ?? 0,
            RectificationVerificationNeedMoreEvidence = verificationStats?.NeedMoreEvidence ?? 0,

            CorrectiveActionCompletionRate = correctiveActionCompletionRate,
            RecentAuditPlans = recentAuditPlans,
            RecentAuditIssues = recentAuditIssues
        };

        _logger.LogInformation("Dashboard summary queried successfully.");

        return Ok(ApiResponse.Ok(result, "仪表盘统计查询成功"));
    }
}
