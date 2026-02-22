using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class ComplianceRule : BaseEntity
{
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // "PolicyActive", "CoverageAmount", etc.
    public int Priority { get; set; } = 1; // 1 = highest, 5 = lowest
    public string Expression { get; set; } = string.Empty; // JSON or C# expression
    public int Severity { get; set; } = 3; // 1-5, 5 being highest
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ApplicableAssetTypes { get; set; } // JSON array or comma-separated
    public string ParametersJson { get; set; } = "{}";
    
    // Navigation properties
    public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    public virtual ICollection<ComplianceAlert> ComplianceAlerts { get; set; } = new List<ComplianceAlert>();
}