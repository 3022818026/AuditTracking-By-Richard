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

    // 分页查询审计计划
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] AuditPlanQueryDto queryDto)
    {
        var query = _dbContext.AuditPlans
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Keyword))
        {
            var keyword = queryDto.Keyword.Trim();

            query = query.Where(x =>
                x.AuditNo.Contains(keyword) ||
                x.Title.Contains(keyword) ||
                (x.Auditee != null && x.Auditee.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Status))
        {
            query = query.Where(x =>
                x.Status == queryDto.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.AuditType))
        {
            query = query.Where(x =>
                x.AuditType == queryDto.AuditType.Trim());
        }

        if (queryDto.StartDate.HasValue)
        {
            query = query.Where(x =>
                x.PlannedDate >= queryDto.StartDate.Value);
        }

        if (queryDto.EndDate.HasValue)
        {
            query = query.Where(x =>
                x.PlannedDate <= queryDto.EndDate.Value);
        }

        var page = queryDto.Page < 1
            ? 1
            : queryDto.Page;

        var pageSize = queryDto.PageSize < 1
            ? 10
            : Math.Min(queryDto.PageSize, 100);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            items,
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(
                total / (double)pageSize)
        });
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
    // 修改审计计划
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAuditPlanDto dto)
    {
        var plan = await _dbContext.AuditPlans.FindAsync(id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        plan.Title = dto.Title.Trim();
        plan.AuditType = dto.AuditType?.Trim();
        plan.PlannedDate = dto.PlannedDate;
        plan.Auditee = dto.Auditee?.Trim();
        plan.Auditor = dto.Auditor?.Trim();
        plan.Status = dto.Status.Trim();
        plan.Result = dto.Result?.Trim();
        plan.Remark = dto.Remark?.Trim();
        plan.UpdatedAt = DateTime.Now;

        await _dbContext.SaveChangesAsync();

        return Ok(plan);
    }
    // 删除审计计划
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _dbContext.AuditPlans.FindAsync(id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        _dbContext.AuditPlans.Remove(plan);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "审计计划删除成功"
        });
    }
}