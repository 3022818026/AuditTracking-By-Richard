using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Entities;

/// <summary>
/// 审计问题整改措施。
/// </summary>
public sealed class CorrectiveAction
{
    /// <summary>
    /// 主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属审计问题 ID。
    /// </summary>
    [Required]
    public int AuditIssueId { get; set; }

    /// <summary>
    /// 整改措施编号。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ActionNo { get; set; } = string.Empty;

    /// <summary>
    /// 整改措施内容。
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>
    /// 整改责任部门。
    /// </summary>
    [MaxLength(100)]
    public string? ResponsibleDepartment { get; set; }

    /// <summary>
    /// 整改责任人。
    /// </summary>
    [MaxLength(100)]
    public string? ResponsiblePerson { get; set; }

    /// <summary>
    /// 计划完成日期。
    /// </summary>
    public DateTime? PlannedCompletionDate { get; set; }

    /// <summary>
    /// 实际完成日期。
    /// </summary>
    public DateTime? ActualCompletionDate { get; set; }

    /// <summary>
    /// 整改完成情况说明。
    /// </summary>
    [MaxLength(4000)]
    public string? CompletionDescription { get; set; }

    /// <summary>
    /// 整改状态：
    /// Draft、Submitted、Approved、Rejected、Completed。
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 提交时间。
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// 审批时间。
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 完成时间。
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建人。
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// 最后修改时间。
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最后修改人。
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 是否已软删除。
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间。
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 删除人。
    /// </summary>
    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// 所属审计问题。
    /// </summary>
    public AuditIssue AuditIssue { get; set; } = null!;

    /// <summary>
    /// 该整改措施包含的验证记录。
    /// </summary>
    public ICollection<RectificationVerification> RectificationVerifications { get; set; }
        = new List<RectificationVerification>();
}
