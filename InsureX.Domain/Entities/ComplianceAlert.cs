using System;

namespace InsureX.Domain.Entities;

public class ComplianceAlert : BaseEntity
{
    public Guid AssetId { get; set; }
    public Guid? RuleId { get; set; }
    public string AlertType { get; set; } = string.Empty; // "Warning", "Critical", "Info"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } = 3; // 1-5
    public string Status { get; set; } = "Active"; // "Active", "Acknowledged", "Resolved"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Navigation properties
    public virtual Asset Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}