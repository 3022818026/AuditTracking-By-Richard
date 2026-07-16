using AuditTracking.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public DatabaseController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();

            return Ok(new
            {
                success = canConnect,
                message = canConnect
                    ? "数据库连接成功"
                    : "数据库连接失败",
                database = "AuditTracking_copy_test"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "数据库连接异常",
                error = ex.Message
            });
        }
    }
}