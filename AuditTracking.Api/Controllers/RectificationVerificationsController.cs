using System.Text.Json;
using AuditTracking.Api.Common;
using AuditTracking.Api.Data;
using AuditTracking.Api.Dtos.RectificationVerifications;
using AuditTracking.Api.Entities;
using AuditTracking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditTracking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/rectification-verifications")]
public sealed class RectificationVerificationsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RectificationVerificationsController> _logger;

    public RectificationVerificationsController(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        ILogger<RectificationVerificationsController> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private static RectificationVerificationOperationLog CreateOperationLog(
        RectificationVerification verification,
        string operationType,
        object? beforeData,
        object? afterData,
        string operatorName,
        string? remark)
    {
        return new RectificationVerificationOperationLog
        {
            RectificationVerificationId = verification.Id,
            VerificationNo = verification.VerificationNo,
            OperationType = operationType,
            BeforeData = beforeData is null ? null : JsonSerializer.Serialize(beforeData),
            AfterData = afterData is null ? null : JsonSerializer.Serialize(afterData),
            Operator = operatorName,
            Remark = remark,
            CreatedAt = DateTime.Now
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] RectificationVerificationQueryDto queryDto)
    {
        // Read query parameters directly (DTO is intentionally empty)
        var q = Request.Query;

        q.TryGetValue("Keyword", out var kwv);
        var keyword = kwv.ToString();

        q.TryGetValue("AuditIssueId", out var aidv);
        int.TryParse(aidv.FirstOrDefault(), out var auditIssueId);

        q.TryGetValue("CorrectiveActionId", out var caidv);
        int.TryParse(caidv.FirstOrDefault(), out var correctiveActionId);

        q.TryGetValue("VerificationResult", out var vrv);
        var verificationResult = vrv.ToString();

        q.TryGetValue("IsPassed", out var ipv);
        bool.TryParse(ipv.FirstOrDefault(), out var isPassedFilter);

        q.TryGetValue("VerifiedDateStart", out var vds);
        DateTime.TryParse(vds.FirstOrDefault(), out var verifiedDateStart);

        q.TryGetValue("VerifiedDateEnd", out var vde);
        DateTime.TryParse(vde.FirstOrDefault(), out var verifiedDateEnd);

        q.TryGetValue("Page", out var pv);
        int page = 1;
        if (!string.IsNullOrWhiteSpace(pv.FirstOrDefault())) int.TryParse(pv.FirstOrDefault(), out page);

        q.TryGetValue("PageSize", out var psv);
        int pageSize = 10;
        if (!string.IsNullOrWhiteSpace(psv.FirstOrDefault())) int.TryParse(psv.FirstOrDefault(), out pageSize);
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var query = _dbContext.RectificationVerifications
            .AsNoTracking()
            .Include(x => x.AuditIssue)
            .Include(x => x.CorrectiveAction)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(x =>
                x.VerificationNo.Contains(k) ||
                x.VerificationComment.Contains(k) ||
                x.Verifier.Contains(k));
        }

        if (auditIssueId > 0)
            query = query.Where(x => x.AuditIssueId == auditIssueId);

        if (correctiveActionId > 0)
            query = query.Where(x => x.CorrectiveActionId == correctiveActionId);

        if (!string.IsNullOrWhiteSpace(verificationResult))
            query = query.Where(x => x.VerificationResult == verificationResult.Trim());

        if (ipv.Count > 0)
            query = query.Where(x => x.IsPassed == isPassedFilter);

        if (verifiedDateStart != default)
            query = query.Where(x => x.VerifiedAt >= verifiedDateStart.Date);

        if (verifiedDateEnd != default)
        {
            var endExclusive = verifiedDateEnd.Date.AddDays(1);
            query = query.Where(x => x.VerifiedAt < endExclusive);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.VerifiedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.AuditIssueId,
                x.CorrectiveActionId,
                x.VerificationNo,
                x.VerificationResult,
                x.VerificationComment,
                x.Verifier,
                x.VerifiedAt,
                x.IsPassed,
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

        return Ok(ApiResponse.Ok(result, "整改验证记录查询成功"));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var verification = await _dbContext.RectificationVerifications
            .AsNoTracking()
            .Include(x => x.AuditIssue)
            .Include(x => x.CorrectiveAction)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.AuditIssueId,
                x.CorrectiveActionId,
                IssueNo = x.AuditIssue.IssueNo,
                IssueTitle = x.AuditIssue.Title,
                ActionNo = x.CorrectiveAction.ActionNo,
                ActionDescription = x.CorrectiveAction.ActionDescription,
                AuditPlanId = x.AuditIssue.AuditPlanId,
                x.VerificationNo,
                x.VerificationResult,
                x.VerificationComment,
                x.Verifier,
                x.VerifiedAt,
                x.IsPassed,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (verification == null)
            return NotFound(ApiResponse.Fail("未找到该整改验证记录"));

        return Ok(ApiResponse.Ok(verification, "整改验证记录查询成功"));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRectificationVerificationDto dto)
    {
        // validate existence
        var issue = await _dbContext.AuditIssues
            .FirstOrDefaultAsync(x => x.Id == dto.AuditIssueId);

        if (issue == null)
            return BadRequest(ApiResponse.Fail("所属审计问题不存在或已被删除"));

        var action = await _dbContext.CorrectiveActions
            .FirstOrDefaultAsync(x => x.Id == dto.CorrectiveActionId);

        if (action == null)
            return BadRequest(ApiResponse.Fail("所属整改措施不存在或已被删除"));

        if (action.AuditIssueId != dto.AuditIssueId)
            return BadRequest(ApiResponse.Fail("该整改措施不属于指定的审计问题"));

        if (action.Status != "Completed")
            return BadRequest(ApiResponse.Fail("只有已完成的整改措施才能进行验证"));

        var verificationNo = dto.VerificationNo.Trim();

        var exists = await _dbContext.RectificationVerifications
            .IgnoreQueryFilters()
            .AnyAsync(x => x.VerificationNo == verificationNo);

        if (exists)
            return BadRequest(ApiResponse.Fail("验证编号已存在，包括回收站中的记录"));

        var now = DateTime.Now;
        var operatorName = _currentUserService.UserName;

        var verification = new RectificationVerification
        {
            AuditIssueId = dto.AuditIssueId,
            CorrectiveActionId = dto.CorrectiveActionId,
            VerificationNo = verificationNo,
            VerificationResult = dto.VerificationResult.Trim(),
            VerificationComment = dto.VerificationComment.Trim(),
            Verifier = dto.Verifier.Trim(),
            VerifiedAt = dto.VerifiedAt ?? DateTime.Now,
            IsPassed = dto.VerificationResult.Trim() == "Passed",
            CreatedAt = now,
            CreatedBy = operatorName,
            IsDeleted = false,
            UpdatedAt = null,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            _dbContext.RectificationVerifications.Add(verification);

            // Update audit issue status according to result
            if (verification.VerificationResult == "Passed")
            {
                issue.Status = "Closed";
                issue.ClosedAt = verification.VerifiedAt;
                issue.UpdatedAt = now;
                issue.UpdatedBy = operatorName;
            }
            else
            {
                issue.Status = "Rectifying";
                issue.ClosedAt = null;
                issue.UpdatedAt = now;
                issue.UpdatedBy = operatorName;
            }

            await _dbContext.SaveChangesAsync();

            var afterSnapshot = new
            {
                verification.Id,
                verification.AuditIssueId,
                verification.CorrectiveActionId,
                verification.VerificationNo,
                verification.VerificationResult,
                verification.VerificationComment,
                verification.Verifier,
                verification.VerifiedAt,
                verification.IsPassed,
                verification.CreatedAt,
                verification.CreatedBy
            };

            var operationLog = CreateOperationLog(
                verification,
                "Create",
                null,
                afterSnapshot,
                operatorName,
                "创建整改验证记录");

            _dbContext.RectificationVerificationOperationLogs.Add(operationLog);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "整改验证记录创建成功。RectificationVerificationId: {Id}, VerificationNo: {VerificationNo}, AuditIssueId: {AuditIssueId}, CorrectiveActionId: {CorrectiveActionId}, VerificationResult: {Result}, Operator: {Operator}",
            verification.Id,
            verification.VerificationNo,
            verification.AuditIssueId,
            verification.CorrectiveActionId,
            verification.VerificationResult,
            operatorName);

        return CreatedAtAction(
            nameof(GetById),
            new { id = verification.Id },
            ApiResponse.Ok(verification, "整改验证记录创建成功"));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRectificationVerificationDto dto)
    {
        var verification = await _dbContext.RectificationVerifications
            .FirstOrDefaultAsync(x => x.Id == id);

        if (verification == null)
            return NotFound(ApiResponse.Fail("未找到该整改验证记录"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeSnapshot = new
        {
            verification.Id,
            verification.AuditIssueId,
            verification.CorrectiveActionId,
            verification.VerificationNo,
            verification.VerificationResult,
            verification.VerificationComment,
            verification.Verifier,
            verification.VerifiedAt,
            verification.IsPassed,
            verification.UpdatedAt,
            verification.UpdatedBy
        };

        // Determine new values
        var newResult = dto.VerificationResult.Trim();
        var newVerifiedAt = dto.VerifiedAt ?? verification.VerifiedAt;
        var newIsPassed = newResult == "Passed";

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            verification.VerificationResult = newResult;
            verification.VerificationComment = dto.VerificationComment.Trim();
            verification.Verifier = dto.Verifier.Trim();
            verification.VerifiedAt = newVerifiedAt;
            verification.IsPassed = newIsPassed;
            verification.UpdatedAt = now;
            verification.UpdatedBy = operatorName;

            // update audit issue
            var issue = await _dbContext.AuditIssues.FirstOrDefaultAsync(x => x.Id == verification.AuditIssueId);

            if (issue != null)
            {
                if (newResult == "Passed")
                {
                    issue.Status = "Closed";
                    issue.ClosedAt = verification.VerifiedAt;
                    issue.UpdatedAt = now;
                    issue.UpdatedBy = operatorName;
                }
                else
                {
                    issue.Status = "Rectifying";
                    issue.ClosedAt = null;
                    issue.UpdatedAt = now;
                    issue.UpdatedBy = operatorName;
                }
            }

            var afterSnapshot = new
            {
                verification.Id,
                verification.AuditIssueId,
                verification.CorrectiveActionId,
                verification.VerificationNo,
                verification.VerificationResult,
                verification.VerificationComment,
                verification.Verifier,
                verification.VerifiedAt,
                verification.IsPassed,
                verification.UpdatedAt,
                verification.UpdatedBy
            };

            var operationLog = CreateOperationLog(
                verification,
                "Update",
                beforeSnapshot,
                afterSnapshot,
                operatorName,
                "修改整改验证记录");

            _dbContext.RectificationVerificationOperationLogs.Add(operationLog);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        _logger.LogInformation(
            "整改验证记录修改成功。RectificationVerificationId: {Id}, VerificationNo: {VerificationNo}, Operator: {Operator}",
            verification.Id,
            verification.VerificationNo,
            operatorName);

        return Ok(ApiResponse.Ok(verification, "整改验证记录修改成功"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var verification = await _dbContext.RectificationVerifications
            .FirstOrDefaultAsync(x => x.Id == id);

        if (verification == null)
            return NotFound(ApiResponse.Fail("未找到该整改验证记录"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeSnapshot = new
        {
            verification.Id,
            verification.VerificationNo,
            verification.VerificationResult,
            verification.Verifier,
            verification.VerifiedAt,
            verification.IsPassed,
            verification.IsDeleted,
            verification.DeletedAt,
            verification.DeletedBy,
            verification.UpdatedAt,
            verification.UpdatedBy
        };

        verification.IsDeleted = true;
        verification.DeletedAt = now;
        verification.DeletedBy = operatorName;
        verification.UpdatedAt = now;
        verification.UpdatedBy = operatorName;

        var afterSnapshot = new
        {
            verification.Id,
            verification.VerificationNo,
            verification.VerificationResult,
            verification.Verifier,
            verification.VerifiedAt,
            verification.IsPassed,
            verification.IsDeleted,
            verification.DeletedAt,
            verification.DeletedBy,
            verification.UpdatedAt,
            verification.UpdatedBy
        };

        var operationLog = CreateOperationLog(
            verification,
            "Delete",
            beforeSnapshot,
            afterSnapshot,
            operatorName,
            "删除整改验证记录");

        _dbContext.RectificationVerificationOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改验证记录删除成功。RectificationVerificationId: {Id}, VerificationNo: {VerificationNo}, Operator: {Operator}",
            verification.Id,
            verification.VerificationNo,
            operatorName);

        return Ok(ApiResponse.Ok("整改验证记录删除成功"));
    }

    [HttpGet("recycle-bin")]
    public async Task<IActionResult> GetRecycleBin()
    {
        var items = await _dbContext.RectificationVerifications
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync();

        return Ok(ApiResponse.Ok(items, "整改验证记录回收站查询成功"));
    }

    [HttpGet("{id:int}/logs")]
    public async Task<IActionResult> GetLogs(int id)
    {
        var exists = await _dbContext.RectificationVerifications
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.Id == id);

        if (!exists)
            return NotFound(ApiResponse.Fail("未找到该整改验证记录"));

        var logs = await _dbContext.RectificationVerificationOperationLogs
            .AsNoTracking()
            .Where(x => x.RectificationVerificationId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.RectificationVerificationId,
                x.VerificationNo,
                x.OperationType,
                x.BeforeData,
                x.AfterData,
                x.Operator,
                x.Remark,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse.Ok(logs, "整改验证操作日志查询成功"));
    }

    [HttpPut("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var verification = await _dbContext.RectificationVerifications
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (verification == null)
            return NotFound(ApiResponse.Fail("未找到该整改验证记录"));

        if (!verification.IsDeleted)
            return BadRequest(ApiResponse.Fail("该整改验证记录未被删除"));

        var operatorName = _currentUserService.UserName;
        var now = DateTime.Now;

        var beforeSnapshot = new
        {
            verification.Id,
            verification.VerificationNo,
            verification.IsDeleted,
            verification.DeletedAt,
            verification.DeletedBy,
            verification.UpdatedAt,
            verification.UpdatedBy
        };

        verification.IsDeleted = false;
        verification.DeletedAt = null;
        verification.DeletedBy = null;
        verification.UpdatedAt = now;
        verification.UpdatedBy = operatorName;

        var afterSnapshot = new
        {
            verification.Id,
            verification.VerificationNo,
            verification.IsDeleted,
            verification.DeletedAt,
            verification.DeletedBy,
            verification.UpdatedAt,
            verification.UpdatedBy
        };

        var operationLog = CreateOperationLog(
            verification,
            "Restore",
            beforeSnapshot,
            afterSnapshot,
            operatorName,
            "恢复整改验证记录");

        _dbContext.RectificationVerificationOperationLogs.Add(operationLog);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "整改验证记录恢复成功。RectificationVerificationId: {Id}, VerificationNo: {VerificationNo}, Operator: {Operator}",
            verification.Id,
            verification.VerificationNo,
            operatorName);

        return Ok(ApiResponse.Ok(verification, "整改验证记录恢复成功"));
    }
}
