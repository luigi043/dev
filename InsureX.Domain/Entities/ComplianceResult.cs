namespace InsureX.Domain.Entities;

public class ComplianceResult : BaseEntity
{
    public int AssetId { get; set; }  // Changed from int? to int
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = ComplianceStatusValues.Pending;
    public string? Message { get; set; }
    public DateTime CheckedAt { get; set; }
    public DateTime? NextCheckDue { get; set; }
    public string? Details { get; set; }
    public int Severity { get; set; } = 0;
    public int? RuleId { get; set; }  // Changed from string? to int?
    public string? CheckedBy { get; set; }
    
    // Navigation properties
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}