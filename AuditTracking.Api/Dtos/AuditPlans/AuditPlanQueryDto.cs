using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditPlans;

public sealed class AuditPlanQueryDto : IValidatableObject
{
    [MaxLength(100, ErrorMessage = "关键词不能超过100个字符")]
    public string? Keyword { get; set; }

    [MaxLength(30, ErrorMessage = "状态不能超过30个字符")]
    public string? Status { get; set; }

    [MaxLength(50, ErrorMessage = "审计类型不能超过50个字符")]
    public string? AuditType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于等于1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
    public int PageSize { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartDate.HasValue &&
            EndDate.HasValue &&
            StartDate.Value.Date > EndDate.Value.Date)
        {
            yield return new ValidationResult(
                "开始日期不能晚于结束日期",
                new[]
                {
                    nameof(StartDate),
                    nameof(EndDate)
                });
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
    }
}