using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.AuditPlans;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Route("api/audit-plans")]
public class AuditPlansController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    private static readonly string[] AllowedStatuses =
    {
        "Draft",
        "InProgress",
        "Completed",
        "Closed",
        "Cancelled"
    };

    public AuditPlansController(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    // 判断状态流转是否合法
    private static bool IsValidStatusTransition(
        string currentStatus,
        string newStatus)
    {
        // 状态未改变时，允许修改其他字段
        if (currentStatus == newStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            "Draft" =>
                newStatus is "InProgress" or "Cancelled",

            "InProgress" =>
                newStatus is "Completed" or "Cancelled",

            "Completed" =>
                newStatus == "Closed",

            "Closed" => false,

            "Cancelled" => false,

            _ => false
        };
    }

    // 获取中文状态名称
    private static string GetStatusName(string status)
    {
        return status switch
        {
            "Draft" => "草稿",
            "InProgress" => "进行中",
            "Completed" => "已完成",
            "Closed" => "已关闭",
            "Cancelled" => "已取消",
            _ => status
        };
    }

    // 分页查询审计计划
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] AuditPlanQueryDto queryDto)
    {
        if (queryDto.StartDate.HasValue &&
            queryDto.EndDate.HasValue &&
            queryDto.StartDate.Value.Date >
            queryDto.EndDate.Value.Date)
        {
            return BadRequest(new
            {
                message = "开始日期不能晚于结束日期"
            });
        }

        var query = _dbContext.AuditPlans
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Keyword))
        {
            var keyword = queryDto.Keyword.Trim();

            query = query.Where(x =>
                x.AuditNo.Contains(keyword) ||
                x.Title.Contains(keyword) ||
                (x.Auditee != null &&
                 x.Auditee.Contains(keyword)) ||
                (x.Auditor != null &&
                 x.Auditor.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Status))
        {
            var status = queryDto.Status.Trim();

            if (!AllowedStatuses.Contains(status))
            {
                return BadRequest(new
                {
                    message = "无效的审计状态"
                });
            }

            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.AuditType))
        {
            var auditType = queryDto.AuditType.Trim();

            query = query.Where(x =>
                x.AuditType == auditType);
        }

        if (queryDto.StartDate.HasValue)
        {
            var startDate =
                queryDto.StartDate.Value.Date;

            query = query.Where(x =>
                x.PlannedDate >= startDate);
        }

        if (queryDto.EndDate.HasValue)
        {
            var endDateExclusive =
                queryDto.EndDate.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.PlannedDate < endDateExclusive);
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
            totalPages = total == 0
                ? 0
                : (int)Math.Ceiling(
                    total / (double)pageSize)
        });
    }

    // 查询审计计划状态统计
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var statistics = await _dbContext.AuditPlans
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToListAsync();

        int GetCount(string status)
        {
            return statistics
                .FirstOrDefault(x =>
                    x.Status == status)
                ?.Count ?? 0;
        }

        var total = statistics.Sum(x => x.Count);

        return Ok(new
        {
            total,
            draft = GetCount("Draft"),
            inProgress = GetCount("InProgress"),
            completed = GetCount("Completed"),
            closed = GetCount("Closed"),
            cancelled = GetCount("Cancelled")
        });
    }

    // 查询审计计划风险统计
    [HttpGet("risk-statistics")]
    public async Task<IActionResult> GetRiskStatistics()
    {
        var today = DateTime.Today;
        var nextSevenDaysExclusive =
            today.AddDays(7);

        var query = _dbContext.AuditPlans
            .AsNoTracking();

        var overdue = await query.CountAsync(x =>
            x.PlannedDate < today &&
            x.Status != "Completed" &&
            x.Status != "Closed" &&
            x.Status != "Cancelled");

        var dueWithinSevenDays =
            await query.CountAsync(x =>
                x.PlannedDate >= today &&
                x.PlannedDate <
                nextSevenDaysExclusive &&
                x.Status != "Completed" &&
                x.Status != "Closed" &&
                x.Status != "Cancelled");

        var completedThisMonth =
            await query.CountAsync(x =>
                x.CompletedAt.HasValue &&
                x.CompletedAt.Value.Year ==
                today.Year &&
                x.CompletedAt.Value.Month ==
                today.Month);

        return Ok(new
        {
            overdue,
            dueWithinSevenDays,
            completedThisMonth
        });
    }

    // 分页查询风险审计计划
    [HttpGet("risk-list")]
    public async Task<IActionResult> GetRiskList(
        [FromQuery] AuditPlanRiskQueryDto queryDto)
    {
        var today = DateTime.Today;
        var nextSevenDaysExclusive =
            today.AddDays(7);

        var query = _dbContext.AuditPlans
            .AsNoTracking()
            .Where(x =>
                x.Status != "Completed" &&
                x.Status != "Closed" &&
                x.Status != "Cancelled");

        if (string.IsNullOrWhiteSpace(queryDto.Type))
        {
            return BadRequest(new
            {
                message = "风险类型不能为空"
            });
        }

        var riskType = queryDto.Type.Trim();

        if (riskType == "Overdue")
        {
            query = query.Where(x =>
                x.PlannedDate < today);
        }
        else if (riskType == "DueSoon")
        {
            query = query.Where(x =>
                x.PlannedDate >= today &&
                x.PlannedDate <
                nextSevenDaysExclusive);
        }
        else
        {
            return BadRequest(new
            {
                message = "无效的风险类型"
            });
        }

        var page = queryDto.Page < 1
            ? 1
            : queryDto.Page;

        var pageSize = queryDto.PageSize < 1
            ? 10
            : Math.Min(queryDto.PageSize, 100);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.PlannedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            type = riskType,
            items,
            page,
            pageSize,
            total,
            totalPages = total == 0
                ? 0
                : (int)Math.Ceiling(
                    total / (double)pageSize)
        });
    }

    // 根据 ID 查询审计计划
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

    // 查询审计计划完整详情
    [HttpGet("{id:int}/detail")]
    public async Task<IActionResult> GetDetail(int id)
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

        var logs = await _dbContext
            .AuditPlanOperationLogs
            .AsNoTracking()
            .Where(x => x.AuditPlanId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = new AuditPlanDetailDto
        {
            Plan = plan,
            Logs = logs
        };

        return Ok(result);
    }

    // 新增审计计划
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAuditPlanDto dto)
    {
        var auditNo = dto.AuditNo.Trim();
        var title = dto.Title.Trim();

        if (string.IsNullOrWhiteSpace(auditNo))
        {
            return BadRequest(new
            {
                message = "审计编号不能为空"
            });
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                message = "审计标题不能为空"
            });
        }

        var auditNoExists = await _dbContext
            .AuditPlans
            .IgnoreQueryFilters()
            .AnyAsync(x => x.AuditNo == auditNo);

        if (auditNoExists)
        {
            return BadRequest(new
            {
                message =
                    "审计编号已存在，包括回收站中的记录"
            });
        }

        var now = DateTime.Now;

        var plan = new AuditPlan
        {
            AuditNo = auditNo,
            Title = title,
            AuditType = dto.AuditType?.Trim(),
            PlannedDate = dto.PlannedDate,
            Auditee = dto.Auditee?.Trim(),
            Auditor = dto.Auditor?.Trim(),
            Remark = dto.Remark?.Trim(),
            Status = "Draft",
            CreatedAt = now,
            CreatedBy = _currentUserService.UserName,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync();

        try
        {
            _dbContext.AuditPlans.Add(plan);
            await _dbContext.SaveChangesAsync();

            var operationLog =
                new AuditPlanOperationLog
                {
                    AuditPlanId = plan.Id,
                    AuditNo = plan.AuditNo,
                    OperationType = "Create",
                    BeforeData = null,
                    AfterData =
                        JsonSerializer.Serialize(plan),
                    Operator =
                        _currentUserService.UserName,
                    CreatedAt = now
                };

            _dbContext.AuditPlanOperationLogs
                .Add(operationLog);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

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
        var plan = await _dbContext.AuditPlans
            .FindAsync(id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        var title = dto.Title.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                message = "审计标题不能为空"
            });
        }

        var status = dto.Status.Trim();

        if (!AllowedStatuses.Contains(status))
        {
            return BadRequest(new
            {
                message = "无效的审计状态"
            });
        }

        if (!IsValidStatusTransition(
                plan.Status,
                status))
        {
            return BadRequest(new
            {
                message =
                    $"不允许从“{GetStatusName(plan.Status)}”" +
                    $"变更为“{GetStatusName(status)}”"
            });
        }

        var result = dto.Result?.Trim();

        if ((status == "Completed" ||
             status == "Closed") &&
            string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(new
            {
                message =
                    "审计计划完成或关闭时，必须填写审计结果"
            });
        }

        var beforeData =
            JsonSerializer.Serialize(plan);

        var oldStatus = plan.Status;
        var now = DateTime.Now;

        plan.Title = title;
        plan.AuditType = dto.AuditType?.Trim();
        plan.PlannedDate = dto.PlannedDate;
        plan.Auditee = dto.Auditee?.Trim();
        plan.Auditor = dto.Auditor?.Trim();
        plan.Status = status;
        plan.Result = result;
        plan.Remark = dto.Remark?.Trim();
        plan.UpdatedAt = now;
        plan.UpdatedBy = _currentUserService.UserName;

        if (oldStatus != "Completed" &&
            status == "Completed")
        {
            plan.CompletedAt = now;
        }

        if (oldStatus != "Closed" &&
            status == "Closed")
        {
            plan.ClosedAt = now;
        }

        var afterData =
            JsonSerializer.Serialize(plan);

        var operationLog =
            new AuditPlanOperationLog
            {
                AuditPlanId = plan.Id,
                AuditNo = plan.AuditNo,
                OperationType = "Update",
                BeforeData = beforeData,
                AfterData = afterData,
                Operator =
                    _currentUserService.UserName,
                CreatedAt = now
            };

        _dbContext.AuditPlanOperationLogs
            .Add(operationLog);

        await _dbContext.SaveChangesAsync();

        return Ok(plan);
    }

    // 单独变更审计计划状态
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        ChangeAuditPlanStatusDto dto)
    {
        var plan = await _dbContext.AuditPlans
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        var newStatus = dto.Status?.Trim();

        if (string.IsNullOrWhiteSpace(newStatus))
        {
            return BadRequest(new
            {
                message = "状态不能为空"
            });
        }

        if (!AllowedStatuses.Contains(newStatus))
        {
            return BadRequest(new
            {
                message = "无效的审计状态"
            });
        }

        var oldStatus = plan.Status;

        if (!IsValidStatusTransition(oldStatus, newStatus))
        {
            return BadRequest(new
            {
                message =
                    $"不允许从“{GetStatusName(oldStatus)}”" +
                    $"变更为“{GetStatusName(newStatus)}”"
            });
        }

        var result = dto.Result?.Trim();
        var remark = dto.Remark?.Trim();


        if ((newStatus == "Completed" ||
             newStatus == "Closed") &&
            string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(new
            {
                message = "审计计划完成或关闭时，必须填写审计结果"
            });
        }

        /*
         * 状态没有发生变化时，不再生成一条重复的状态日志。
         */
        if (oldStatus == newStatus)
        {
            return BadRequest(new
            {
                message = "新状态与当前状态相同，无需重复变更"
            });
        }

        var beforeData = JsonSerializer.Serialize(plan);
        var now = DateTime.Now;

        plan.Status = newStatus;
        plan.Result = result;
        plan.Remark = remark;
        plan.UpdatedAt = now;

        // 首次变更为已完成时记录完成时间
        if (newStatus == "Completed" &&
            !plan.CompletedAt.HasValue)
        {
            plan.CompletedAt = now;
        }

        // 首次变更为已关闭时记录关闭时间
        if (newStatus == "Closed" &&
            !plan.ClosedAt.HasValue)
        {
            plan.ClosedAt = now;
        }

        var afterData = JsonSerializer.Serialize(plan);

        var operationLog = new AuditPlanOperationLog
        {
            AuditPlanId = plan.Id,
            AuditNo = plan.AuditNo,
            OperationType = "StatusChange",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = _currentUserService.UserName,
            CreatedAt = now
        };

        _dbContext.AuditPlanOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "审计计划状态变更成功",
            data = plan
        });
    }

    // 查询已删除的审计计划
    [HttpGet("recycle-bin")]
    public async Task<IActionResult> GetRecycleBin()
    {
        var plans = await _dbContext.AuditPlans
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync();

        return Ok(plans);
    }

    // 恢复已删除的审计计划
    [HttpPut("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var plan = await _dbContext.AuditPlans
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        if (!plan.IsDeleted)
        {
            return BadRequest(new
            {
                message = "该审计计划未被删除"
            });
        }

        var beforeData =
            JsonSerializer.Serialize(plan);

        var now = DateTime.Now;

        plan.IsDeleted = false;
        plan.DeletedAt = null;
        plan.DeletedBy = null;
        plan.UpdatedAt = now;
        plan.UpdatedBy = _currentUserService.UserName;

        var afterData =
            JsonSerializer.Serialize(plan);

        var operationLog =
            new AuditPlanOperationLog
            {
                AuditPlanId = plan.Id,
                AuditNo = plan.AuditNo,
                OperationType = "Restore",
                BeforeData = beforeData,
                AfterData = afterData,
                Operator =
                    _currentUserService.UserName,
                CreatedAt = now
            };

        _dbContext.AuditPlanOperationLogs
            .Add(operationLog);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "审计计划恢复成功",
            data = plan
        });
    }

    // 软删除审计计划
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _dbContext.AuditPlans
            .FindAsync(id);

        if (plan == null)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        var beforeData =
            JsonSerializer.Serialize(plan);

        var now = DateTime.Now;

        plan.IsDeleted = true;
        plan.DeletedAt = now;
        plan.DeletedBy = _currentUserService.UserName;
        plan.UpdatedAt = now;
        plan.UpdatedBy = _currentUserService.UserName;

        var afterData =
            JsonSerializer.Serialize(plan);

        var operationLog =
            new AuditPlanOperationLog
            {
                AuditPlanId = plan.Id,
                AuditNo = plan.AuditNo,
                OperationType = "Delete",
                BeforeData = beforeData,
                AfterData = afterData,
                Operator =
                    _currentUserService.UserName,
                CreatedAt = now
            };

        _dbContext.AuditPlanOperationLogs
            .Add(operationLog);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "审计计划删除成功"
        });
    }

    // 查询指定审计计划的操作日志
    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetOperationLogs(
        int id)
    {
        var planExists = await _dbContext.AuditPlans
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);

        if (!planExists)
        {
            return NotFound(new
            {
                message = "未找到该审计计划"
            });
        }

        var logs = await _dbContext
            .AuditPlanOperationLogs
            .AsNoTracking()
            .Where(x => x.AuditPlanId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(logs);
    }
}