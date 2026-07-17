using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditPlans;

public class UpdateAuditPlanDto
{
    [Required(ErrorMessage = "审计标题不能为空")]
    [MaxLength(200, ErrorMessage = "审计标题不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AuditType { get; set; }

    [Required(ErrorMessage = "计划日期不能为空")]
    public DateTime PlannedDate { get; set; }

    [MaxLength(200)]
    public string? Auditee { get; set; }

    [MaxLength(200)]
    public string? Auditor { get; set; }

    [Required(ErrorMessage = "审计状态不能为空")]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    [MaxLength(100)]
    public string? Result { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
}