using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.Auth;

public sealed class LoginRequestDto
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50, ErrorMessage = "用户名不能超过50个字符")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    [MaxLength(100, ErrorMessage = "密码不能超过100个字符")]
    public string Password { get; set; } = string.Empty;
}
