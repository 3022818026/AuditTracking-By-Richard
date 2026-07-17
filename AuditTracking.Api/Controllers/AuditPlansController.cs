using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.AuditPlans;
using AuditTracking.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Route("api/audit-plans")]
public class AuditPlansController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AuditPlansController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 查询全部审计计划
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var plans = await _dbContext.AuditPlans
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(plans);
    }

    // 根据 ID 查询详情
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _dbContext.AuditPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        return Ok(plan);
    }

    // 新增审计计划
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAuditPlanDto dto)
    {
        var auditNoExists = await _dbContext.AuditPlans
            .AnyAsync(x => x.AuditNo == dto.AuditNo);

        if (auditNoExists)
        {
            return BadRequest(new
            {
                message = "审计编号已存在"
            });
        }

        var plan = new AuditPlan
        {
            AuditNo = dto.AuditNo.Trim(),
            Title = dto.Title.Trim(),
            AuditType = dto.AuditType?.Trim(),
            PlannedDate = dto.PlannedDate,
            Auditee = dto.Auditee?.Trim(),
            Auditor = dto.Auditor?.Trim(),
            Remark = dto.Remark?.Trim(),
            Status = "Draft",
            CreatedAt = DateTime.Now
        };

        _dbContext.AuditPlans.Add(plan);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = plan.Id },
            plan);
    }
}