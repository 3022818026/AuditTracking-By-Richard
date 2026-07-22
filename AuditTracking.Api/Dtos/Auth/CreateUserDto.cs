using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.Auth;

public sealed class CreateUserDto : IValidatableObject
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50, ErrorMessage = "用户名不能超过50个字符")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "显示名称不能为空")]
    [MaxLength(100, ErrorMessage = "显示名称不能超过100个字符")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    [MaxLength(100, ErrorMessage = "密码不能超过100个字符")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "角色不能为空")]
    [MaxLength(30, ErrorMessage = "角色不能超过30个字符")]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(UserName) &&
            UserName.Any(char.IsWhiteSpace))
        {
            yield return new ValidationResult(
                "用户名中不能包含空格",
                [nameof(UserName)]);
        }

        if (!string.IsNullOrWhiteSpace(Role) &&
            Role is not "Admin" and not "User")
        {
            yield return new ValidationResult(
                "角色必须为 Admin 或 User",
                [nameof(Role)]);
        }
    }
}
