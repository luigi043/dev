using System;

namespace InsureX.Domain.Entities;

public class ComplianceHistory : BaseEntity
{
    public Guid AssetId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid? ComplianceCheckId { get; set; }
    public string AdditionalData { get; set; } = "{}"; // JSON for extra context
    
    // Navigation properties
    public virtual Asset Asset { get; set; }
    public virtual ComplianceCheck? ComplianceCheck { get; set; }
}