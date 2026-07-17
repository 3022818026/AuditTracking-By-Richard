using System.ComponentModel.DataAnnotations;

namespace AuditTracking.Api.Dtos.AuditPlans;

public sealed class AuditPlanRiskQueryDto : IValidatableObject
{
    [Required(ErrorMessage = "风险类型不能为空")]
    [MaxLength(30, ErrorMessage = "风险类型不能超过30个字符")]
    public string Type { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于等于1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "每页数量必须在1到100之间")]
    public int PageSize { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Type))
        {
            var allowedTypes = new[]
            {
                "Overdue",
                "DueSoon"
            };

            if (!allowedTypes.Contains(Type.Trim()))
            {
                yield return new ValidationResult(
                    "无效的风险类型",
                    new[] { nameof(Type) });
            }
        }
    }
}