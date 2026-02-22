namespace InsureX.Domain.Entities;

public class ComplianceCheck : BaseEntity
{
    public int AssetId { get; set; }  // Changed from Guid to int
    public int? RuleId { get; set; }  // Changed from Guid? to int?
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompliant { get; set; }
    public double Score { get; set; }
    public string? Details { get; set; }
    public string? Violations { get; set; }
    public string? Recommendations { get; set; }
    public string CheckedBy { get; set; } = "system";
    
    // Navigation properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual ComplianceRule? Rule { get; set; }
}