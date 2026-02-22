namespace InsureX.Domain.Entities;

public class ComplianceResult : BaseEntity
{
    public int AssetId { get; set; }
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = ComplianceStatusValues.Pending;
    public string? Message { get; set; }
    public DateTime CheckedAt { get; set; }
    public DateTime? NextCheckDue { get; set; }
    public string? Details { get; set; } // JSON string for flexible data
    public int Severity { get; set; } = 0; // 0=Info, 1=Warning, 2=Critical
    public string? RuleId { get; set; }
    public string? CheckedBy { get; set; }
    
    // Navigation properties
    public virtual Asset? Asset { get; set; }
}