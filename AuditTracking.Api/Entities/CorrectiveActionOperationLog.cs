using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Entities;

/// <summary>
/// 整改措施操作日志。
/// </summary>
public sealed class CorrectiveActionOperationLog
{
    /// <summary>
    /// 主键。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 整改措施 ID。
    /// </summary>
    public int CorrectiveActionId { get; set; }

    /// <summary>
    /// 整改措施编号。
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ActionNo { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型：Create、Update、StatusChange、Delete、Restore。
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// 操作前的数据，使用 JSON 保存。
    /// </summary>
    public string? BeforeData { get; set; }

    /// <summary>
    /// 操作后的数据，使用 JSON 保存。
    /// </summary>
    public string? AfterData { get; set; }

    /// <summary>
    /// 操作人。
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Operator { get; set; } = "System";

    /// <summary>
    /// 操作备注。
    /// 状态变更时可记录备注。
    /// </summary>
    [MaxLength(1000)]
    public string? Remark { get; set; }

    /// <summary>
    /// 操作时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
