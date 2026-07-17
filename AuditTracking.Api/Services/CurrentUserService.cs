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
            var userName = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["X-User-Name"]
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(userName)
                ? "System"
                : userName.Trim();
        }
    }
}