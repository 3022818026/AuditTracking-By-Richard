using AuditTracking.Api.Entities;

namespace AuditTracking.Api.Dtos.AuditPlans;

public class AuditPlanDetailDto
{
    public AuditPlan Plan { get; set; } = null!;

    public List<AuditPlanOperationLog> Logs { get; set; } = [];
}