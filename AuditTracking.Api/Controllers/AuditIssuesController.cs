using AuditTracking.Api.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.AuditIssues;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Route("api/audit-issues")]
public sealed class AuditIssuesController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditIssuesController> _logger;

    public AuditIssuesController(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<AuditIssuesController> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// 判断审计问题状态流转是否合法。
    /// </summary>
    private static bool IsValidStatusTransition(
        string currentStatus,
        string newStatus)
    {
        return currentStatus switch
        {
            "Open" =>
                newStatus is "Rectifying" or "Rejected",

            "Rectifying" =>
                newStatus is "PendingVerification" or "Rejected",

            "PendingVerification" =>
                newStatus is "Closed" or "Rectifying",

            "Closed" => false,

            "Rejected" => false,

            _ => false
        };
    }

    /// <summary>
    /// 获取状态的中文名称。
    /// </summary>
    private static string GetStatusName(string status)
    {
        return status switch
        {
            "Open" => "待整改",
            "Rectifying" => "整改中",
            "PendingVerification" => "待验证",
            "Closed" => "已关闭",
            "Rejected" => "已驳回",
            _ => status
        };
    }

    /// <summary>
    /// 分页查询审计问题。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] AuditIssueQueryDto queryDto)
    {
        var query = _dbContext.AuditIssues
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Keyword))
        {
            var keyword = queryDto.Keyword.Trim();

            query = query.Where(x =>
                x.IssueNo.Contains(keyword) ||
                x.Title.Contains(keyword) ||
                x.Description.Contains(keyword) ||
                (x.ResponsibleDepartment != null &&
                 x.ResponsibleDepartment.Contains(keyword)) ||
                (x.ResponsiblePerson != null &&
                 x.ResponsiblePerson.Contains(keyword)));
        }

        if (queryDto.AuditPlanId.HasValue)
        {
            query = query.Where(x =>
                x.AuditPlanId == queryDto.AuditPlanId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Status))
        {
            var status = queryDto.Status.Trim();

            query = query.Where(x =>
                x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Severity))
        {
            var severity = queryDto.Severity.Trim();

            query = query.Where(x =>
                x.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.IssueType))
        {
            var issueType = queryDto.IssueType.Trim();

            query = query.Where(x =>
                x.IssueType == issueType);
        }

        if (queryDto.DueDateStart.HasValue)
        {
            var startDate =
                queryDto.DueDateStart.Value.Date;

            query = query.Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value >= startDate);
        }

        if (queryDto.DueDateEnd.HasValue)
        {
            var endDateExclusive =
                queryDto.DueDateEnd.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value < endDateExclusive);
        }

        if (queryDto.IsOverdue == true)
        {
            var today = DateTime.Today;

            query = query.Where(x =>
                x.DueDate.HasValue &&
                x.DueDate.Value < today &&
                x.Status != "Closed" &&
                x.Status != "Rejected");
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .Select(x => new
            {
                x.Id,
                x.AuditPlanId,
                x.IssueNo,
                x.Title,
                x.Description,
                x.IssueType,
                x.Severity,
                x.ResponsibleDepartment,
                x.ResponsiblePerson,
                x.DueDate,
                x.Status,
                x.ClosedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        var result = new
        {
            items,
            page = queryDto.Page,
            pageSize = queryDto.PageSize,
            total,
            totalPages = total == 0
                ? 0
                : (int)Math.Ceiling(
                    total / (double)queryDto.PageSize)
        };

        return Ok(
            ApiResponse.Ok(
                result,
                "审计问题查询成功"));
    }

    /// <summary>
    /// 根据 ID 查询审计问题。
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var issue = await _dbContext.AuditIssues
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.AuditPlanId,
                AuditNo = x.AuditPlan.AuditNo,
                AuditTitle = x.AuditPlan.Title,
                x.IssueNo,
                x.Title,
                x.Description,
                x.IssueType,
                x.Severity,
                x.ResponsibleDepartment,
                x.ResponsiblePerson,
                x.DueDate,
                x.Status,
                x.ClosedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (issue == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        return Ok(
            ApiResponse.Ok(
                issue,
                "审计问题查询成功"));
    }

    /// <summary>
    /// 新增审计问题。
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAuditIssueDto dto)
    {
        var auditPlanExists = await _dbContext.AuditPlans
            .AnyAsync(x => x.Id == dto.AuditPlanId);

        if (!auditPlanExists)
        {
            return BadRequest(
                ApiResponse.Fail(
                    "所属审计计划不存在或已被删除"));
        }

        var issueNo = dto.IssueNo.Trim();

        var issueNoExists = await _dbContext.AuditIssues
            .IgnoreQueryFilters()
            .AnyAsync(x => x.IssueNo == issueNo);

        if (issueNoExists)
        {
            return BadRequest(
                ApiResponse.Fail(
                    "问题编号已存在，包括回收站中的记录"));
        }

        var now = DateTime.Now;
        var operatorName = _currentUserService.UserName;

        var issue = new AuditIssue
        {
            AuditPlanId = dto.AuditPlanId,
            IssueNo = issueNo,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            IssueType = dto.IssueType?.Trim(),
            Severity = dto.Severity.Trim(),
            ResponsibleDepartment =
                dto.ResponsibleDepartment?.Trim(),
            ResponsiblePerson =
                dto.ResponsiblePerson?.Trim(),
            DueDate = dto.DueDate,
            Status = "Open",
            ClosedAt = null,
            CreatedAt = now,
            CreatedBy = operatorName,
            UpdatedAt = null,
            UpdatedBy = null,
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.AuditIssues.Add(issue);
            await _dbContext.SaveChangesAsync();

            var operationLog = new AuditIssueOperationLog
            {
                AuditIssueId = issue.Id,
                IssueNo = issue.IssueNo,
                OperationType = "Create",
                BeforeData = null,
                AfterData = JsonSerializer.Serialize(issue),
                Operator = operatorName,
                Remark = null,
                CreatedAt = now
            };

            _dbContext.AuditIssueOperationLogs.Add(operationLog);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "审计问题创建成功。AuditIssueId: {AuditIssueId}, IssueNo: {IssueNo}, AuditPlanId: {AuditPlanId}, Operator: {Operator}",
            issue.Id,
            issue.IssueNo,
            issue.AuditPlanId,
            operatorName);

        return CreatedAtAction(
            nameof(GetById),
            new { id = issue.Id },
            ApiResponse.Ok(
                issue,
                "审计问题创建成功"));
    }

    /// <summary>
    /// 修改审计问题。
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAuditIssueDto dto)
    {
        var issue = await _dbContext.AuditIssues
            .FirstOrDefaultAsync(x => x.Id == id);

        if (issue == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(issue);

        issue.Title = dto.Title.Trim();
        issue.Description = dto.Description.Trim();
        issue.IssueType = dto.IssueType?.Trim();
        issue.Severity = dto.Severity.Trim();
        issue.ResponsibleDepartment =
            dto.ResponsibleDepartment?.Trim();
        issue.ResponsiblePerson =
            dto.ResponsiblePerson?.Trim();
        issue.DueDate = dto.DueDate;
        issue.UpdatedAt = now;
        issue.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(issue);

        var operationLog = new AuditIssueOperationLog
        {
            AuditIssueId = issue.Id,
            IssueNo = issue.IssueNo,
            OperationType = "Update",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.AuditIssueOperationLogs.Add(operationLog);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "审计问题修改成功。AuditIssueId: {AuditIssueId}, IssueNo: {IssueNo}, Operator: {Operator}",
            issue.Id,
            issue.IssueNo,
            operatorName);

        return Ok(
            ApiResponse.Ok(
                issue,
                "审计问题修改成功"));
    }

    /// <summary>
    /// 单独变更审计问题状态。
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id,
        ChangeAuditIssueStatusDto dto)
    {
        var issue = await _dbContext.AuditIssues
            .FirstOrDefaultAsync(x => x.Id == id);

        if (issue == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        var newStatus = dto.Status.Trim();
        var oldStatus = issue.Status;

        if (oldStatus == newStatus)
        {
            return BadRequest(
                ApiResponse.Fail(
                    "新状态与当前状态相同，无需重复变更"));
        }

        if (!IsValidStatusTransition(
                oldStatus,
                newStatus))
        {
            return BadRequest(
                ApiResponse.Fail(
                    $"不允许从“{GetStatusName(oldStatus)}”" +
                    $"变更为“{GetStatusName(newStatus)}”"));
        }

        var operatorName =
            _currentUserService.UserName;

        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(issue);

        issue.Status = newStatus;
        issue.UpdatedAt = now;
        issue.UpdatedBy = operatorName;

        if (newStatus == "Closed")
        {
            issue.ClosedAt = now;
        }

        var afterData = JsonSerializer.Serialize(issue);

        var operationLog = new AuditIssueOperationLog
        {
            AuditIssueId = issue.Id,
            IssueNo = issue.IssueNo,
            OperationType = "StatusChange",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = dto.Remark?.Trim(),
            CreatedAt = now
        };

        _dbContext.AuditIssueOperationLogs.Add(operationLog);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "审计问题状态变更成功。AuditIssueId: {AuditIssueId}, IssueNo: {IssueNo}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, Operator: {Operator}, Remark: {Remark}",
            issue.Id,
            issue.IssueNo,
            oldStatus,
            newStatus,
            operatorName,
            dto.Remark?.Trim());

        return Ok(
            ApiResponse.Ok(
                issue,
                "审计问题状态变更成功"));
    }

    /// <summary>
    /// 软删除审计问题。
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var issue = await _dbContext.AuditIssues
            .FirstOrDefaultAsync(x => x.Id == id);

        if (issue == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(issue);

        issue.IsDeleted = true;
        issue.DeletedAt = now;
        issue.DeletedBy = operatorName;
        issue.UpdatedAt = now;
        issue.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(issue);

        var operationLog = new AuditIssueOperationLog
        {
            AuditIssueId = issue.Id,
            IssueNo = issue.IssueNo,
            OperationType = "Delete",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.AuditIssueOperationLogs.Add(operationLog);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "审计问题删除成功。AuditIssueId: {AuditIssueId}, IssueNo: {IssueNo}, Operator: {Operator}",
            issue.Id,
            issue.IssueNo,
            operatorName);

        return Ok(
            ApiResponse.Ok(
                "审计问题删除成功"));
    }

    /// <summary>
    /// 查询审计问题回收站。
    /// </summary>
    [HttpGet("recycle-bin")]
    public async Task<IActionResult> GetRecycleBin()
    {
        var issues = await _dbContext.AuditIssues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync();

        return Ok(
            ApiResponse.Ok(
                issues,
                "审计问题回收站查询成功"));
    }

    /// <summary>
    /// 恢复已删除的审计问题。
    /// </summary>
    [HttpPut("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var issue = await _dbContext.AuditIssues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (issue == null)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        if (!issue.IsDeleted)
        {
            return BadRequest(
                ApiResponse.Fail(
                    "该审计问题未被删除"));
        }

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(issue);

        issue.IsDeleted = false;
        issue.DeletedAt = null;
        issue.DeletedBy = null;
        issue.UpdatedAt = now;
        issue.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(issue);

        var operationLog = new AuditIssueOperationLog
        {
            AuditIssueId = issue.Id,
            IssueNo = issue.IssueNo,
            OperationType = "Restore",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.AuditIssueOperationLogs.Add(operationLog);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "审计问题恢复成功。AuditIssueId: {AuditIssueId}, IssueNo: {IssueNo}, Operator: {Operator}",
            issue.Id,
            issue.IssueNo,
            operatorName);

        return Ok(
            ApiResponse.Ok(
                issue,
                "审计问题恢复成功"));
    }

    /// <summary>
    /// 查询指定审计问题的操作日志。
    /// </summary>
    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetOperationLogs(int id)
    {
        var issueExists = await _dbContext.AuditIssues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);

        if (!issueExists)
        {
            return NotFound(
                ApiResponse.Fail(
                    "未找到该审计问题"));
        }

        var logs = await _dbContext.AuditIssueOperationLogs
            .AsNoTracking()
            .Where(x => x.AuditIssueId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(
            ApiResponse.Ok(
                logs,
                "审计问题操作日志查询成功"));
    }
}