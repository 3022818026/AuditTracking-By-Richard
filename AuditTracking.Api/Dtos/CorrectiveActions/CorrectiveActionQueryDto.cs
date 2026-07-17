using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.CorrectiveActions;

public sealed class CorrectiveActionQueryDto : IValidatableObject
{
    [MaxLength(100, ErrorMessage = "关键词不能超过100个字符")]
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "审计问题ID必须大于0")]
    public int? AuditIssueId { get; set; }

    [MaxLength(30, ErrorMessage = "整改状态不能超过30个字符")]
    public string? Status { get; set; }

    [MaxLength(100, ErrorMessage = "责任部门不能超过100个字符")]
    public string? ResponsibleDepartment { get; set; }

    [MaxLength(100, ErrorMessage = "责任人不能超过100个字符")]
    public string? ResponsiblePerson { get; set; }

    public DateTime? PlannedDateStart { get; set; }

    public DateTime? PlannedDateEnd { get; set; }

    /// <summary>
    /// 是否只查询已逾期且未完成的整改措施。
    /// </summary>
    public bool? IsOverdue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于等于1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
    public int PageSize { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PlannedDateStart.HasValue &&
            PlannedDateEnd.HasValue &&
            PlannedDateStart.Value.Date >
            PlannedDateEnd.Value.Date)
        {
            yield return new ValidationResult(
                "计划完成开始日期不能晚于结束日期",
                new[]
                {
                    nameof(PlannedDateStart),
                    nameof(PlannedDateEnd)
                });
        }

        if (!string.IsNullOrWhiteSpace(Status))
        {
            var allowedStatuses = new[]
            {
                "Draft",
                "Submitted",
                "Approved",
                "Rejected",
                "Completed"
            };

            if (!allowedStatuses.Contains(Status.Trim()))
            {
                yield return new ValidationResult(
                    "整改状态必须为 Draft、Submitted、Approved、Rejected 或 Completed",
                    new[] { nameof(Status) });
            }
        }

        if (PlannedDateStart.HasValue &&
            PlannedDateStart.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "计划完成开始日期必须在2000年至2100年之间",
                new[] { nameof(PlannedDateStart) });
        }

        if (PlannedDateEnd.HasValue &&
            PlannedDateEnd.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "计划完成结束日期必须在2000年至2100年之间",
                new[] { nameof(PlannedDateEnd) });
        }
    }
}