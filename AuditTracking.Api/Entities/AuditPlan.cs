using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditTracking.Api.Entities;

[Table("AuditPlans")]
public class AuditPlan
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AuditNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AuditType { get; set; }

    public DateTime PlannedDate { get; set; }

    [MaxLength(200)]
    public string? Auditee { get; set; }

    [MaxLength(200)]
    public string? Auditor { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";

    [MaxLength(100)]
    public string? Result { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "System";

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    /// <summary>
    /// 该审计计划包含的审计问题。
    /// </summary>
    public ICollection<AuditIssue> AuditIssues { get; set; }
        = new List<AuditIssue>();

}
