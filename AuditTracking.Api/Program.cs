using AuditTracking.Api.Data;
using Microsoft.EntityFrameworkCore;
using AuditTracking.Api.Services;
using AuditTracking.Api.Middleware;
using AuditTracking.Api.Common;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var errors = context.ModelState
                    .Where(item =>
                        item.Value?.Errors.Count > 0)
                    .ToDictionary(
                        item => item.Key,
                        item => item.Value!.Errors
                            .Select(error =>
                                string.IsNullOrWhiteSpace(
                                    error.ErrorMessage)
                                    ? "参数格式不正确"
                                    : error.ErrorMessage)
                            .ToArray());

                var response = ApiResponse.Fail(
                    message: "请求参数校验失败",
                    errors: errors,
                    traceId:
                        context.HttpContext
                            .TraceIdentifier);

                return new BadRequestObjectResult(
                    response);
            };
    });
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "未找到数据库连接字符串 DefaultConnection。");

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(connectionString));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
