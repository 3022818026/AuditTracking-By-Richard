using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditIssues;

public sealed class AuditIssueQueryDto : IValidatableObject
{
    [MaxLength(100, ErrorMessage = "关键词不能超过100个字符")]
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "审计计划ID必须大于0")]
    public int? AuditPlanId { get; set; }

    [MaxLength(30, ErrorMessage = "问题状态不能超过30个字符")]
    public string? Status { get; set; }

    [MaxLength(30, ErrorMessage = "严重程度不能超过30个字符")]
    public string? Severity { get; set; }

    [MaxLength(50, ErrorMessage = "问题类型不能超过50个字符")]
    public string? IssueType { get; set; }

    public DateTime? DueDateStart { get; set; }

    public DateTime? DueDateEnd { get; set; }

    /// <summary>
    /// 是否只查询已逾期问题。
    /// </summary>
    public bool? IsOverdue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于等于1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
    public int PageSize { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (DueDateStart.HasValue &&
            DueDateEnd.HasValue &&
            DueDateStart.Value.Date > DueDateEnd.Value.Date)
        {
            yield return new ValidationResult(
                "整改期限开始日期不能晚于结束日期",
                new[]
                {
                    nameof(DueDateStart),
                    nameof(DueDateEnd)
                });
        }

        if (!string.IsNullOrWhiteSpace(Status))
        {
            var allowedStatuses = new[]
            {
                "Open",
                "Rectifying",
                "PendingVerification",
                "Closed",
                "Rejected"
            };

            if (!allowedStatuses.Contains(Status.Trim()))
            {
                yield return new ValidationResult(
                    "问题状态必须为 Open、Rectifying、PendingVerification、Closed 或 Rejected",
                    new[] { nameof(Status) });
            }
        }

        if (!string.IsNullOrWhiteSpace(Severity))
        {
            var allowedSeverities = new[]
            {
                "Low",
                "Medium",
                "High",
                "Critical"
            };

            if (!allowedSeverities.Contains(Severity.Trim()))
            {
                yield return new ValidationResult(
                    "严重程度必须为 Low、Medium、High 或 Critical",
                    new[] { nameof(Severity) });
            }
        }

        if (DueDateStart.HasValue &&
            DueDateStart.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "整改期限开始日期必须在2000年至2100年之间",
                new[] { nameof(DueDateStart) });
        }

        if (DueDateEnd.HasValue &&
            DueDateEnd.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "整改期限结束日期必须在2000年至2100年之间",
                new[] { nameof(DueDateEnd) });
        }
    }
}