using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.CorrectiveActions;

public sealed class UpdateCorrectiveActionDto : IValidatableObject
{
    [Required(ErrorMessage = "整改措施内容不能为空")]
    [MaxLength(4000, ErrorMessage = "整改措施内容不能超过4000个字符")]
    public string ActionDescription { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "责任部门不能超过100个字符")]
    public string? ResponsibleDepartment { get; set; }

    [MaxLength(100, ErrorMessage = "责任人不能超过100个字符")]
    public string? ResponsiblePerson { get; set; }

    public DateTime? PlannedCompletionDate { get; set; }

    [MaxLength(4000, ErrorMessage = "完成情况说明不能超过4000个字符")]
    public string? CompletionDescription { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PlannedCompletionDate.HasValue &&
            PlannedCompletionDate.Value.Year is < 2000 or > 2100)
        {
            yield return new ValidationResult(
                "计划完成日期必须在2000年至2100年之间",
                new[] { nameof(PlannedCompletionDate) });
        }
    }
}