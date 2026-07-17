using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.RectificationVerifications;

public sealed class RectificationVerificationQueryDto
    : IValidatableObject
{
    [MaxLength(100, ErrorMessage = "关键词不能超过100个字符")]
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "审计问题ID必须大于0")]
    public int? AuditIssueId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "整改措施ID必须大于0")]
    public int? CorrectiveActionId { get; set; }

    [MaxLength(30, ErrorMessage = "验证结果不能超过30个字符")]
    public string? VerificationResult { get; set; }

    public bool? IsPassed { get; set; }

    public DateTime? VerifiedDateStart { get; set; }

    public DateTime? VerifiedDateEnd { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于等于1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
    public int PageSize { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (VerifiedDateStart.HasValue &&
            VerifiedDateEnd.HasValue &&
            VerifiedDateStart.Value.Date >
            VerifiedDateEnd.Value.Date)
        {
            yield return new ValidationResult(
                "验证开始日期不能晚于结束日期",
                new[]
                {
                    nameof(VerifiedDateStart),
                    nameof(VerifiedDateEnd)
                });
        }

        if (!string.IsNullOrWhiteSpace(
                VerificationResult))
        {
            var allowedResults = new[]
            {
                "Passed",
                "Failed",
                "NeedMoreEvidence"
            };

            if (!allowedResults.Contains(
                    VerificationResult.Trim()))
            {
                yield return new ValidationResult(
                    "验证结果必须为 Passed、Failed 或 NeedMoreEvidence",
                    new[] { nameof(VerificationResult) });
            }
        }

        if (VerifiedDateStart.HasValue &&
            VerifiedDateStart.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "验证开始日期必须在2000年至2100年之间",
                new[] { nameof(VerifiedDateStart) });
        }

        if (VerifiedDateEnd.HasValue &&
            VerifiedDateEnd.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "验证结束日期必须在2000年至2100年之间",
                new[] { nameof(VerifiedDateEnd) });
        }
    }
}
