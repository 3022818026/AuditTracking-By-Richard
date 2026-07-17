using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Entities;

/// <summary>
/// 整改验证记录。
/// </summary>
public sealed class RectificationVerification
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
    /// 所属整改措施 ID。
    /// </summary>
    [Required]
    public int CorrectiveActionId { get; set; }

    /// <summary>
    /// 验证编号。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string VerificationNo { get; set; } = string.Empty;

    /// <summary>
    /// 验证结果：
    /// Passed、Failed、NeedMoreEvidence。
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string VerificationResult { get; set; } = string.Empty;

    /// <summary>
    /// 验证意见。
    /// </summary>
    [Required]
    [MaxLength(4000)]
    public string VerificationComment { get; set; } = string.Empty;

    /// <summary>
    /// 验证人。
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Verifier { get; set; } = string.Empty;

    /// <summary>
    /// 验证时间。
    /// </summary>
    public DateTime VerifiedAt { get; set; }

    /// <summary>
    /// 是否通过。
    /// </summary>
    public bool IsPassed { get; set; }

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
    /// 所属整改措施。
    /// </summary>
    public CorrectiveAction CorrectiveAction { get; set; } = null!;
}