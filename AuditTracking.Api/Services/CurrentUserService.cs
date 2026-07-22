using System.Security.Claims;

namespace AuditTracking.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserName
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var claimsUserName = httpContext?.User
                .FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(claimsUserName))
            {
                claimsUserName = httpContext?.User
                    .FindFirst("username")?.Value;
            }

            if (!string.IsNullOrWhiteSpace(claimsUserName))
                return claimsUserName.Trim();

            var headerUserName = httpContext?
                .Request.Headers["X-User-Name"]
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(headerUserName)
                ? "System"
                : headerUserName.Trim();
        }
    }
}
