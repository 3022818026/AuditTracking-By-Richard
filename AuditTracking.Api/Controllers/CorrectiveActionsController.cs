using AuditTracking.Api.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.CorrectiveActions;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/corrective-actions")]
public sealed class CorrectiveActionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CorrectiveActionsController> _logger;

    public CorrectiveActionsController(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<CorrectiveActionsController> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private static bool IsValidStatusTransition(
        string currentStatus,
        string newStatus)
    {
        if (currentStatus == newStatus)
            return true;

        return currentStatus switch
        {
            "Draft" => newStatus == "Submitted",
            "Submitted" => newStatus == "Approved" || newStatus == "Rejected",
            "Rejected" => newStatus == "Draft",
            "Approved" => newStatus == "Completed",
            "Completed" => false,
            _ => false
        };
    }

    private static string GetStatusName(string status)
    {
        return status switch
        {
            "Draft" => "草稿",
            "Submitted" => "已提交",
            "Approved" => "已批准",
            "Rejected" => "已驳回",
            "Completed" => "已完成",
            _ => status
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] CorrectiveActionQueryDto queryDto)
    {
        var query = _dbContext.CorrectiveActions
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Keyword))
        {
            var kw = queryDto.Keyword.Trim();
            query = query.Where(x =>
                x.ActionNo.Contains(kw) ||
                x.ActionDescription.Contains(kw) ||
                (x.ResponsibleDepartment != null && x.ResponsibleDepartment.Contains(kw)) ||
                (x.ResponsiblePerson != null && x.ResponsiblePerson.Contains(kw)));
        }

        if (queryDto.AuditIssueId.HasValue)
        {
            query = query.Where(x => x.AuditIssueId == queryDto.AuditIssueId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Status))
        {
            query = query.Where(x => x.Status == queryDto.Status.Trim());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.ResponsibleDepartment))
        {
            query = query.Where(x => x.ResponsibleDepartment == queryDto.ResponsibleDepartment.Trim());
        }

        if (!string.IsNullOrWhiteSpace(queryDto.ResponsiblePerson))
        {
            query = query.Where(x => x.ResponsiblePerson == queryDto.ResponsiblePerson.Trim());
        }

        if (queryDto.PlannedDateStart.HasValue)
        {
            var start = queryDto.PlannedDateStart.Value.Date;
            query = query.Where(x => x.PlannedCompletionDate.HasValue && x.PlannedCompletionDate.Value >= start);
        }

        if (queryDto.PlannedDateEnd.HasValue)
        {
            var endExclusive = queryDto.PlannedDateEnd.Value.Date.AddDays(1);
            query = query.Where(x => x.PlannedCompletionDate.HasValue && x.PlannedCompletionDate.Value < endExclusive);
        }

        if (queryDto.IsOverdue == true)
        {
            var today = DateTime.Today;
            query = query.Where(x => x.PlannedCompletionDate.HasValue && x.PlannedCompletionDate.Value < today && x.Status != "Completed");
        }

        var total = await query.CountAsync();

        var page = queryDto.Page < 1 ? 1 : queryDto.Page;
        var pageSize = queryDto.PageSize < 1 ? 10 : Math.Min(queryDto.PageSize, 100);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.AuditIssueId,
                x.ActionNo,
                x.ActionDescription,
                x.ResponsibleDepartment,
                x.ResponsiblePerson,
                x.PlannedCompletionDate,
                x.ActualCompletionDate,
                x.CompletionDescription,
                x.Status,
                x.SubmittedAt,
                x.ApprovedAt,
                x.CompletedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        var result = new
        {
            items,
            page,
            pageSize,
            total,
            totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(ApiResponse.Ok(result, "整改措施查询成功"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var action = await _dbContext.CorrectiveActions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.AuditIssueId,
                IssueNo = x.AuditIssue.IssueNo,
                IssueTitle = x.AuditIssue.Title,
                AuditPlanId = x.AuditIssue.AuditPlanId,
                x.ActionNo,
                x.ActionDescription,
                x.ResponsibleDepartment,
                x.ResponsiblePerson,
                x.PlannedCompletionDate,
                x.ActualCompletionDate,
                x.CompletionDescription,
                x.Status,
                x.SubmittedAt,
                x.ApprovedAt,
                x.CompletedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (action == null)
            return NotFound(ApiResponse.Fail("未找到该整改措施"));

        return Ok(ApiResponse.Ok(action, "整改措施查询成功"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCorrectiveActionDto dto)
    {
        var issueExists = await _dbContext.AuditIssues
            .AnyAsync(x => x.Id == dto.AuditIssueId);

        if (!issueExists)
            return BadRequest(ApiResponse.Fail("所属审计问题不存在或已被删除"));

        var actionNo = dto.ActionNo.Trim();

        var actionNoExists = await _dbContext.CorrectiveActions
            .IgnoreQueryFilters()
            .AnyAsync(x => x.ActionNo == actionNo);

        if (actionNoExists)
            return BadRequest(ApiResponse.Fail("整改措施编号已存在，包括回收站中的记录"));

        var now = DateTime.Now;
        var operatorName = _currentUserService.UserName;

        var action = new CorrectiveAction
        {
            AuditIssueId = dto.AuditIssueId,
            ActionNo = actionNo,
            ActionDescription = dto.ActionDescription.Trim(),
            ResponsibleDepartment = dto.ResponsibleDepartment?.Trim(),
            ResponsiblePerson = dto.ResponsiblePerson?.Trim(),
            PlannedCompletionDate = dto.PlannedCompletionDate,
            ActualCompletionDate = null,
            CompletionDescription = dto.CompletionDescription?.Trim(),
            Status = "Draft",
            SubmittedAt = null,
            ApprovedAt = null,
            CompletedAt = null,
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
            _dbContext.CorrectiveActions.Add(action);
            await _dbContext.SaveChangesAsync();

            var operationLog = new CorrectiveActionOperationLog
            {
                CorrectiveActionId = action.Id,
                ActionNo = action.ActionNo,
                OperationType = "Create",
                BeforeData = null,
                AfterData = JsonSerializer.Serialize(action),
                Operator = operatorName,
                Remark = null,
                CreatedAt = now
            };

            _dbContext.CorrectiveActionOperationLogs.Add(operationLog);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "整改措施创建成功。CorrectiveActionId: {CorrectiveActionId}, ActionNo: {ActionNo}, AuditIssueId: {AuditIssueId}, Operator: {Operator}",
            action.Id,
            action.ActionNo,
            action.AuditIssueId,
            operatorName);

        return CreatedAtAction(nameof(GetById), new { id = action.Id }, ApiResponse.Ok(action, "整改措施创建成功"));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCorrectiveActionDto dto)
    {
        var action = await _dbContext.CorrectiveActions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (action == null)
            return NotFound(ApiResponse.Fail("未找到该整改措施"));

        if (action.Status == "Completed")
            return BadRequest(ApiResponse.Fail("已完成的整改措施不能修改"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(action);

        action.ActionDescription = dto.ActionDescription.Trim();
        action.ResponsibleDepartment = dto.ResponsibleDepartment?.Trim();
        action.ResponsiblePerson = dto.ResponsiblePerson?.Trim();
        action.PlannedCompletionDate = dto.PlannedCompletionDate;
        action.CompletionDescription = dto.CompletionDescription?.Trim();
        action.UpdatedAt = now;
        action.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(action);

        var operationLog = new CorrectiveActionOperationLog
        {
            CorrectiveActionId = action.Id,
            ActionNo = action.ActionNo,
            OperationType = "Update",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.CorrectiveActionOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改措施修改成功。CorrectiveActionId: {CorrectiveActionId}, ActionNo: {ActionNo}, Operator: {Operator}",
            action.Id,
            action.ActionNo,
            operatorName);

        return Ok(ApiResponse.Ok(action, "整改措施修改成功"));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, ChangeCorrectiveActionStatusDto dto)
    {
        var action = await _dbContext.CorrectiveActions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (action == null)
            return NotFound(ApiResponse.Fail("未找到该整改措施"));

        var newStatus = dto.Status.Trim();
        var oldStatus = action.Status;

        if (oldStatus == newStatus)
            return BadRequest(ApiResponse.Fail("新状态与当前状态相同，无需重复变更"));

        if (!IsValidStatusTransition(oldStatus, newStatus))
            return BadRequest(ApiResponse.Fail($"不允许从“{GetStatusName(oldStatus)}”变更为“{GetStatusName(newStatus)}”"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(action);

        if (newStatus == "Submitted")
        {
            action.SubmittedAt = now;
        }
        else if (newStatus == "Approved")
        {
            action.ApprovedAt = now;
        }
        else if (newStatus == "Completed")
        {
            action.CompletionDescription = dto.CompletionDescription?.Trim();
            action.CompletedAt = now;
            action.ActualCompletionDate = now;
        }
        else if (newStatus == "Draft")
        {
            action.ApprovedAt = null;
            action.CompletedAt = null;
            action.ActualCompletionDate = null;
        }

        action.Status = newStatus;
        action.UpdatedAt = now;
        action.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(action);

        var operationLog = new CorrectiveActionOperationLog
        {
            CorrectiveActionId = action.Id,
            ActionNo = action.ActionNo,
            OperationType = "StatusChange",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = dto.Remark?.Trim(),
            CreatedAt = now
        };

        _dbContext.CorrectiveActionOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改措施状态变更成功。CorrectiveActionId: {CorrectiveActionId}, ActionNo: {ActionNo}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, Operator: {Operator}, Remark: {Remark}",
            action.Id,
            action.ActionNo,
            oldStatus,
            newStatus,
            operatorName,
            dto.Remark?.Trim());

        return Ok(ApiResponse.Ok(action, "整改措施状态变更成功"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var action = await _dbContext.CorrectiveActions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (action == null)
            return NotFound(ApiResponse.Fail("未找到该整改措施"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(action);

        action.IsDeleted = true;
        action.DeletedAt = now;
        action.DeletedBy = operatorName;
        action.UpdatedAt = now;
        action.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(action);

        var operationLog = new CorrectiveActionOperationLog
        {
            CorrectiveActionId = action.Id,
            ActionNo = action.ActionNo,
            OperationType = "Delete",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.CorrectiveActionOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改措施删除成功。CorrectiveActionId: {CorrectiveActionId}, ActionNo: {ActionNo}, Operator: {Operator}",
            action.Id,
            action.ActionNo,
            operatorName);

        return Ok(ApiResponse.Ok("整改措施删除成功"));
    }

    [HttpGet("recycle-bin")]
    public async Task<IActionResult> GetRecycleBin()
    {
        var items = await _dbContext.CorrectiveActions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync();

        return Ok(ApiResponse.Ok(items, "整改措施回收站查询成功"));
    }

    [HttpPut("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var action = await _dbContext.CorrectiveActions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (action == null)
            return NotFound(ApiResponse.Fail("未找到该整改措施"));

        if (!action.IsDeleted)
            return BadRequest(ApiResponse.Fail("该整改措施未被删除"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeData = JsonSerializer.Serialize(action);

        action.IsDeleted = false;
        action.DeletedAt = null;
        action.DeletedBy = null;
        action.UpdatedAt = now;
        action.UpdatedBy = operatorName;

        var afterData = JsonSerializer.Serialize(action);

        var operationLog = new CorrectiveActionOperationLog
        {
            CorrectiveActionId = action.Id,
            ActionNo = action.ActionNo,
            OperationType = "Restore",
            BeforeData = beforeData,
            AfterData = afterData,
            Operator = operatorName,
            Remark = null,
            CreatedAt = now
        };

        _dbContext.CorrectiveActionOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改措施恢复成功。CorrectiveActionId: {CorrectiveActionId}, ActionNo: {ActionNo}, Operator: {Operator}",
            action.Id,
            action.ActionNo,
            operatorName);

        return Ok(ApiResponse.Ok(action, "整改措施恢复成功"));
    }

    /// <summary>
    /// 查询指定整改措施的操作日志。
    /// </summary>
    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetOperationLogs(int id)
    {
        var actionExists = await _dbContext.CorrectiveActions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);

        if (!actionExists)
        {
            return NotFound(ApiResponse.Fail("未找到该整改措施"));
        }

        var logs = await _dbContext.CorrectiveActionOperationLogs
            .AsNoTracking()
            .Where(x => x.CorrectiveActionId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse.Ok(logs, "整改措施操作日志查询成功"));
    }
}
