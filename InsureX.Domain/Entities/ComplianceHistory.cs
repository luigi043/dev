namespace InsureX.Domain.Entities;

public class ComplianceHistory : BaseEntity
{
    public int AssetId { get; set; }  // Changed from Guid to int
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? ComplianceCheckId { get; set; }  // Changed from Guid? to int?
    public string AdditionalData { get; set; } = "{}";
    
    // Navigation properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual ComplianceCheck? ComplianceCheck { get; set; }
}