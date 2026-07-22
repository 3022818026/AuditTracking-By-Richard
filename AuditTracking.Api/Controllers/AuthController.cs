using System.Security.Claims;
using AuditTracking.Api.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.Auth;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Services;
using AuditTracking.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IPasswordHasher<AppUser> passwordHasher,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var userName = dto.UserName.Trim();
        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(x => x.UserName == userName);

        if (user == null)
            return Unauthorized(ApiResponse.Fail("用户名或密码错误"));

        if (!user.IsActive)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse.Fail("用户已被禁用"));

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return Unauthorized(ApiResponse.Fail("用户名或密码错误"));

        var now = DateTime.Now;
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            user.UpdatedAt = now;
            user.UpdatedBy = user.UserName;
        }

        user.LastLoginAt = now;
        await _dbContext.SaveChangesAsync();

        var response = _jwtTokenService.CreateToken(user);

        _logger.LogInformation(
            "用户登录成功。UserId: {UserId}, UserName: {UserName}, Role: {Role}",
            user.Id,
            user.UserName,
            user.Role);

        return Ok(ApiResponse.Ok(response, "登录成功"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

        var user = await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return Unauthorized(ApiResponse.Fail("未找到当前用户"));

        if (!user.IsActive)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse.Fail("用户已被禁用"));

        var result = ToCurrentUserDto(user);
        return Ok(ApiResponse.Ok(result, "当前用户信息查询成功"));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var userName = dto.UserName.Trim();
        var displayName = dto.DisplayName.Trim();
        var role = dto.Role.Trim();

        if (role is not "Admin" and not "User")
            return BadRequest(ApiResponse.Fail("角色必须为 Admin 或 User"));

        var exists = await _dbContext.AppUsers
            .AsNoTracking()
            .AnyAsync(x => x.UserName == userName);

        if (exists)
            return BadRequest(ApiResponse.Fail("用户名已存在"));

        var user = new AppUser
        {
            UserName = userName,
            DisplayName = displayName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.Now,
            CreatedBy = _currentUserService.UserName
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        _dbContext.AppUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "用户创建成功。UserId: {UserId}, UserName: {UserName}, Role: {Role}, Operator: {Operator}",
            user.Id,
            user.UserName,
            user.Role,
            _currentUserService.UserName);

        return Ok(ApiResponse.Ok(ToCurrentUserDto(user), "用户创建成功"));
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized(ApiResponse.Fail("未登录或登录已过期"));

        var user = await _dbContext.AppUsers
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return Unauthorized(ApiResponse.Fail("未找到当前用户"));

        if (!user.IsActive)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse.Fail("用户已被禁用"));

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.CurrentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
            return BadRequest(ApiResponse.Fail("当前密码错误"));

        if (string.Equals(
                dto.CurrentPassword,
                dto.NewPassword,
                StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse.Fail("新密码不能与当前密码相同"));
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.UpdatedAt = DateTime.Now;
        user.UpdatedBy = user.UserName;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "用户密码修改成功。UserId: {UserId}, UserName: {UserName}",
            user.Id,
            user.UserName);

        return Ok(ApiResponse.Ok("密码修改成功，请重新登录"));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out userId);
    }

    private static CurrentUserDto ToCurrentUserDto(AppUser user)
    {
        return new CurrentUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Role = user.Role,
            LastLoginAt = user.LastLoginAt
        };
    }
}
