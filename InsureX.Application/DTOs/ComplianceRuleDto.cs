using System;

namespace InsureX.Application.DTOs;

public class ComplianceRuleDto
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsActive { get; set; }
    public int? DaysToExpiry { get; set; }
    public int Priority { get; set; }
    public string? ApplicableAssetTypes { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}