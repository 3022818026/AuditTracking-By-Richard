using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuditTracking.Api.Dtos.Auth;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuditTracking.Api.Services.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public LoginResponseDto CreateToken(AppUser user)
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer))
            throw new InvalidOperationException("JWT Issuer 未配置。");

        if (string.IsNullOrWhiteSpace(_options.Audience))
            throw new InvalidOperationException("JWT Audience 未配置。");

        var signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey ?? string.Empty);
        if (signingKeyBytes.Length < 32)
            throw new InvalidOperationException("JWT SigningKey 未配置或长度不足，必须至少为32字节。");

        if (_options.ExpirationMinutes <= 0)
            throw new InvalidOperationException("JWT ExpirationMinutes 必须大于0。");

        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.GivenName, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(signingKeyBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}
