namespace AuditTracking.Api.Dtos.AuditPlans;

public class AuditPlanQueryDto
{
    public string? Keyword { get; set; }

    public string? Status { get; set; }

    public string? AuditType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}