namespace AuditTracking.Api.Dtos.AuditPlans;

public class AuditPlanRiskQueryDto
{
    public string Type { get; set; } = "Overdue";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}