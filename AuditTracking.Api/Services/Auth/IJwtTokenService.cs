using AuditTracking.Api.Dtos.Auth;
using AuditTracking.Api.Entities;

namespace AuditTracking.Api.Services.Auth;

public interface IJwtTokenService
{
    LoginResponseDto CreateToken(AppUser user);
}
