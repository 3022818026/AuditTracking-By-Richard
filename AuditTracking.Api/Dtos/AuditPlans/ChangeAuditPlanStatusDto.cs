using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditPlans;

public class ChangeAuditPlanStatusDto
{
    [Required(ErrorMessage = "状态不能为空")]
    [MaxLength(30, ErrorMessage = "状态长度不能超过30个字符")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "审计结果长度不能超过2000个字符")]
    public string? Result { get; set; }

    [MaxLength(1000, ErrorMessage = "备注长度不能超过1000个字符")]
    public string? Remark { get; set; }
}