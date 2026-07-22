using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.Auth;

public sealed class ChangePasswordDto : IValidatableObject
{
    [Required(ErrorMessage = "当前密码不能为空")]
    [MinLength(6, ErrorMessage = "当前密码不能少于6个字符")]
    [MaxLength(100, ErrorMessage = "当前密码不能超过100个字符")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "新密码不能为空")]
    [MinLength(6, ErrorMessage = "新密码不能少于6个字符")]
    [MaxLength(100, ErrorMessage = "新密码不能超过100个字符")]
    public string NewPassword { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(CurrentPassword) &&
            string.Equals(
                CurrentPassword,
                NewPassword,
                StringComparison.Ordinal))
        {
            yield return new ValidationResult(
                "新密码不能与当前密码相同",
                [nameof(NewPassword)]);
        }
    }
}
