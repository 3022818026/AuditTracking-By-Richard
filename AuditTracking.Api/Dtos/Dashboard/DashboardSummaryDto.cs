namespace AuditTracking.Api.Dtos.Dashboard;

public sealed class DashboardSummaryDto
{
    public int AuditPlanTotal { get; set; }
    public int AuditPlanDraft { get; set; }
    public int AuditPlanInProgress { get; set; }
    public int AuditPlanCompleted { get; set; }
    public int AuditPlanClosed { get; set; }
    public int AuditPlanCancelled { get; set; }
    public int AuditPlanOverdue { get; set; }

    public int AuditIssueTotal { get; set; }
    public int AuditIssueOpen { get; set; }
    public int AuditIssueRectifying { get; set; }
    public int AuditIssuePendingVerification { get; set; }
    public int AuditIssueClosed { get; set; }
    public int AuditIssueRejected { get; set; }
    public int AuditIssueOverdue { get; set; }

    public int CorrectiveActionTotal { get; set; }
    public int CorrectiveActionDraft { get; set; }
    public int CorrectiveActionSubmitted { get; set; }
    public int CorrectiveActionApproved { get; set; }
    public int CorrectiveActionRejected { get; set; }
    public int CorrectiveActionCompleted { get; set; }
    public int CorrectiveActionOverdue { get; set; }

    public int RectificationVerificationTotal { get; set; }
    public int RectificationVerificationPassed { get; set; }
    public int RectificationVerificationFailed { get; set; }
    public int RectificationVerificationNeedMoreEvidence { get; set; }

    public decimal CorrectiveActionCompletionRate { get; set; }

    public List<RecentAuditPlanDto> RecentAuditPlans { get; set; } = [];
    public List<RecentAuditIssueDto> RecentAuditIssues { get; set; } = [];
}

public sealed class RecentAuditPlanDto
{
    public int Id { get; set; }
    public string AuditNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public string? Auditee { get; set; }
    public string? Auditor { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RecentAuditIssueDto
{
    public int Id { get; set; }
    public string IssueNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? ResponsibleDepartment { get; set; }
    public string? ResponsiblePerson { get; set; }
    public int AuditPlanId { get; set; }
    public DateTime CreatedAt { get; set; }
}
