using System.Data.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Services.Auth;

public static class DefaultAdminInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DefaultAdminInitializer));

        var userName = configuration["InitialAdmin:UserName"]?.Trim();
        var displayName = configuration["InitialAdmin:DisplayName"]?.Trim();
        var password = configuration["InitialAdmin:Password"];

        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "未配置 InitialAdmin 用户名或密码，跳过默认管理员初始化。");
            return;
        }

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider
                .GetRequiredService<IPasswordHasher<AppUser>>();

            var adminExists = await dbContext.AppUsers
                .AsNoTracking()
                .AnyAsync(x => x.Role == "Admin");

            if (adminExists)
                return;

            var admin = new AppUser
            {
                UserName = userName,
                DisplayName = string.IsNullOrWhiteSpace(displayName)
                    ? userName
                    : displayName,
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, password);
            dbContext.AppUsers.Add(admin);
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "默认管理员创建成功。UserName: {UserName}",
                admin.UserName);
        }
        catch (DbException)
        {
            logger.LogError(
                "默认管理员初始化失败，请确认数据库已应用包含 AppUsers 表的 Migration。");
        }
    }
}
