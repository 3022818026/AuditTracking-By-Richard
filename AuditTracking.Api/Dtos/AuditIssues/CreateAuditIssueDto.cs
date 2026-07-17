using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditIssues;

public sealed class CreateAuditIssueDto : IValidatableObject
{
    [Required(ErrorMessage = "所属审计计划不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "所属审计计划ID必须大于0")]
    public int AuditPlanId { get; set; }

    [Required(ErrorMessage = "问题编号不能为空")]
    [MaxLength(50, ErrorMessage = "问题编号不能超过50个字符")]
    public string IssueNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "问题标题不能为空")]
    [MaxLength(200, ErrorMessage = "问题标题不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "问题描述不能为空")]
    [MaxLength(4000, ErrorMessage = "问题描述不能超过4000个字符")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "问题类型不能超过50个字符")]
    public string? IssueType { get; set; }

    [Required(ErrorMessage = "严重程度不能为空")]
    [MaxLength(30, ErrorMessage = "严重程度不能超过30个字符")]
    public string Severity { get; set; } = "Medium";

    [MaxLength(100, ErrorMessage = "责任部门不能超过100个字符")]
    public string? ResponsibleDepartment { get; set; }

    [MaxLength(100, ErrorMessage = "责任人不能超过100个字符")]
    public string? ResponsiblePerson { get; set; }

    public DateTime? DueDate { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var allowedSeverities = new[]
        {
            "Low",
            "Medium",
            "High",
            "Critical"
        };

        if (!string.IsNullOrWhiteSpace(Severity) &&
            !allowedSeverities.Contains(Severity.Trim()))
        {
            yield return new ValidationResult(
                "严重程度必须为 Low、Medium、High 或 Critical",
                new[] { nameof(Severity) });
        }

        if (!string.IsNullOrWhiteSpace(IssueNo) &&
            IssueNo.Trim().Contains(' '))
        {
            yield return new ValidationResult(
                "问题编号中不能包含空格",
                new[] { nameof(IssueNo) });
        }

        if (DueDate.HasValue &&
            DueDate.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "整改期限必须在2000年至2100年之间",
                new[] { nameof(DueDate) });
        }
    }
}