namespace InsureX.Domain.Entities;

public class ComplianceAlert : BaseEntity
{
    public int AssetId { get; set; }  // Changed from Guid to int
    public int? RuleId { get; set; }  // Changed from Guid? to int?
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } = 3;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Navigation properties
    public virtual Asset Asset { get; set; } = null!;
    public virtual ComplianceRule? Rule { get; set; }
}