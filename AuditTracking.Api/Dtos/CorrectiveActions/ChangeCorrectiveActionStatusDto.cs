using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.CorrectiveActions;

public sealed class ChangeCorrectiveActionStatusDto : IValidatableObject
{
    [Required(ErrorMessage = "整改状态不能为空")]
    [MaxLength(30, ErrorMessage = "整改状态不能超过30个字符")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "完成情况说明不能超过4000个字符")]
    public string? CompletionDescription { get; set; }

    [MaxLength(1000, ErrorMessage = "备注不能超过1000个字符")]
    public string? Remark { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var allowedStatuses = new[]
        {
            "Draft",
            "Submitted",
            "Approved",
            "Rejected",
            "Completed"
        };

        var status = Status?.Trim();

        if (!string.IsNullOrWhiteSpace(status) &&
            !allowedStatuses.Contains(status))
        {
            yield return new ValidationResult(
                "整改状态必须为 Draft、Submitted、Approved、Rejected 或 Completed",
                new[] { nameof(Status) });
        }

        if (status == "Completed" &&
            string.IsNullOrWhiteSpace(CompletionDescription))
        {
            yield return new ValidationResult(
                "整改措施完成时，必须填写完成情况说明",
                new[] { nameof(CompletionDescription) });
        }
    }
}