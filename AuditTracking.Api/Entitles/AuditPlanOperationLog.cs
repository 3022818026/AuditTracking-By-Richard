using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditTracking.Api.Entities;

[Table("AuditPlanOperationLogs")]
public class AuditPlanOperationLog
{
    [Key]
    public int Id { get; set; }

    public int AuditPlanId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AuditNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string OperationType { get; set; } = string.Empty;

    public string? BeforeData { get; set; }

    public string? AfterData { get; set; }

    [MaxLength(100)]
    public string Operator { get; set; } = "System";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
