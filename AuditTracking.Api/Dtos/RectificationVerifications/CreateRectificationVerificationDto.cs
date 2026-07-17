using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.RectificationVerifications;

public sealed class CreateRectificationVerificationDto : IValidatableObject
{
    [Required(ErrorMessage = "所属审计问题不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "审计问题ID必须大于0")]
    public int AuditIssueId { get; set; }

    [Required(ErrorMessage = "所属整改措施不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "整改措施ID必须大于0")]
    public int CorrectiveActionId { get; set; }

    [Required(ErrorMessage = "验证编号不能为空")]
    [MaxLength(50, ErrorMessage = "验证编号不能超过50个字符")]
    public string VerificationNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证结果不能为空")]
    [MaxLength(30, ErrorMessage = "验证结果不能超过30个字符")]
    public string VerificationResult { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证意见不能为空")]
    [MaxLength(4000, ErrorMessage = "验证意见不能超过4000个字符")]
    public string VerificationComment { get; set; } = string.Empty;

    [Required(ErrorMessage = "验证人不能为空")]
    [MaxLength(100, ErrorMessage = "验证人不能超过100个字符")]
    public string Verifier { get; set; } = string.Empty;

    public DateTime? VerifiedAt { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var allowedResults = new[]
        {
            "Passed",
            "Failed",
            "NeedMoreEvidence"
        };

        var result = VerificationResult?.Trim();

        if (!string.IsNullOrWhiteSpace(result) &&
            !allowedResults.Contains(result))
        {
            yield return new ValidationResult(
                "验证结果必须为 Passed、Failed 或 NeedMoreEvidence",
                new[] { nameof(VerificationResult) });
        }

        if (!string.IsNullOrWhiteSpace(VerificationNo) &&
            VerificationNo.Trim().Contains(' '))
        {
            yield return new ValidationResult(
                "验证编号中不能包含空格",
                new[] { nameof(VerificationNo) });
        }

        if (VerifiedAt.HasValue &&
            VerifiedAt.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "验证时间必须在2000年至2100年之间",
                new[] { nameof(VerifiedAt) });
        }
    }
}