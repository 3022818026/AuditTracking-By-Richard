using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Entities;

public sealed class RectificationVerificationOperationLog
{
    public int Id { get; set; }

    public int RectificationVerificationId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VerificationNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string OperationType { get; set; } = string.Empty;

    public string? BeforeData { get; set; }

    public string? AfterData { get; set; }

    [Required]
    [MaxLength(100)]
    public string Operator { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}