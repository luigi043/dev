namespace InsureX.Domain.Entities;

public class ComplianceRule : BaseEntity
{
    public int? TenantId { get; set; }  // Added for tenant isolation
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string Expression { get; set; } = string.Empty;
    public int Severity { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ApplicableAssetTypes { get; set; }
    public string ParametersJson { get; set; } = "{}";
    
    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    public virtual ICollection<ComplianceAlert> ComplianceAlerts { get; set; } = new List<ComplianceAlert>();
}