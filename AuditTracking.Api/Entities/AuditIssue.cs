using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Entities;

/// <summary>
/// 审计问题。
/// </summary>
public sealed class AuditIssue
{
    /// <summary>
    /// 主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属审计计划 ID。
    /// </summary>
    [Required]
    public int AuditPlanId { get; set; }

    /// <summary>
    /// 问题编号。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string IssueNo { get; set; } = string.Empty;

    /// <summary>
    /// 问题标题。
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 问题详细描述。
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 问题类型。
    /// </summary>
    [MaxLength(50)]
    public string? IssueType { get; set; }

    /// <summary>
    /// 严重程度：Low、Medium、High、Critical。
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// 责任部门。
    /// </summary>
    [MaxLength(100)]
    public string? ResponsibleDepartment { get; set; }

    /// <summary>
    /// 责任人。
    /// </summary>
    [MaxLength(100)]
    public string? ResponsiblePerson { get; set; }

    /// <summary>
    /// 要求整改完成日期。
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// 问题状态：
    /// Open、Rectifying、PendingVerification、Closed、Rejected。
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// 关闭时间。
    /// </summary>
    public DateTime? ClosedAt { get; set; }

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
    /// 所属审计计划。
    /// </summary>
    public AuditPlan AuditPlan { get; set; } = null!;

    /// <summary>
    /// 该审计问题包含的整改措施。
    /// </summary>
    public ICollection<CorrectiveAction> CorrectiveActions { get; set; }
        = new List<CorrectiveAction>();

    /// <summary>
    /// 该审计问题包含的整改验证记录。
    /// </summary>
    public ICollection<RectificationVerification> RectificationVerifications { get; set; }
        = new List<RectificationVerification>();
}
