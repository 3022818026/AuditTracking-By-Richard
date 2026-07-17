using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditIssues;

public sealed class ChangeAuditIssueStatusDto : IValidatableObject
{
    [Required(ErrorMessage = "问题状态不能为空")]
    [MaxLength(30, ErrorMessage = "问题状态不能超过30个字符")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "备注不能超过1000个字符")]
    public string? Remark { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var allowedStatuses = new[]
        {
            "Open",
            "Rectifying",
            "PendingVerification",
            "Closed",
            "Rejected"
        };

        if (!string.IsNullOrWhiteSpace(Status) &&
            !allowedStatuses.Contains(Status.Trim()))
        {
            yield return new ValidationResult(
                "问题状态必须为 Open、Rectifying、PendingVerification、Closed 或 Rejected",
                new[] { nameof(Status) });
        }
    }
}