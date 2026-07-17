using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditPlans;

public sealed class UpdateAuditPlanDto : IValidatableObject
{
    [Required(ErrorMessage = "审计标题不能为空")]
    [MaxLength(200, ErrorMessage = "审计标题不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "审计类型不能超过50个字符")]
    public string? AuditType { get; set; }

    [Required(ErrorMessage = "计划日期不能为空")]
    public DateTime PlannedDate { get; set; }

    [MaxLength(100, ErrorMessage = "受审部门不能超过100个字符")]
    public string? Auditee { get; set; }

    [MaxLength(100, ErrorMessage = "审计人员不能超过100个字符")]
    public string? Auditor { get; set; }

    [Required(ErrorMessage = "审计状态不能为空")]
    [MaxLength(30, ErrorMessage = "审计状态不能超过30个字符")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "审计结果不能超过2000个字符")]
    public string? Result { get; set; }

    [MaxLength(1000, ErrorMessage = "备注不能超过1000个字符")]
    public string? Remark { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PlannedDate == default)
        {
            yield return new ValidationResult(
                "计划日期不能为空",
                new[] { nameof(PlannedDate) });
        }

        if (PlannedDate.Year < 2000 ||
            PlannedDate.Year > 2100)
        {
            yield return new ValidationResult(
                "计划日期必须在2000年至2100年之间",
                new[] { nameof(PlannedDate) });
        }

        if (!string.IsNullOrWhiteSpace(Status))
        {
            var allowedStatuses = new[]
            {
                "Draft",
                "InProgress",
                "Completed",
                "Closed",
                "Cancelled"
            };

            if (!allowedStatuses.Contains(Status.Trim()))
            {
                yield return new ValidationResult(
                    "无效的审计状态",
                    new[] { nameof(Status) });
            }
        }

        if ((Status == "Completed" ||
             Status == "Closed") &&
            string.IsNullOrWhiteSpace(Result))
        {
            yield return new ValidationResult(
                "审计计划完成或关闭时，必须填写审计结果",
                new[] { nameof(Result) });
        }
    }
}