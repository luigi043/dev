using System;

namespace InsureX.Domain.Entities;

public class ComplianceCheck : BaseEntity
{
    public Guid AssetId { get; set; }
    public Guid? RuleId { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompliant { get; set; }
    public double Score { get; set; } // 0-100
    public string? Details { get; set; }
    public string? Violations { get; set; } // JSON array of violations
    public string? Recommendations { get; set; }
    public string CheckedBy { get; set; } = "system"; // User ID or "system"
    
    // Navigation properties
    public virtual Asset Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}