using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class ComplianceRule : BaseEntity
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // Policy, Payment, Inspection, Documentation
    public string Condition { get; set; } = string.Empty; // JSON condition
    public string Action { get; set; } = string.Empty; // JSON action
    public int Severity { get; set; } = 1; // 1-Low, 2-Medium, 3-High, 4-Critical
    public bool IsActive { get; set; } = true;
    public int? DaysToExpiry { get; set; } // For expiry-based rules
    public string? ApplicablePolicyTypes { get; set; } // JSON array or comma separated
    public string? ApplicableAssetTypes { get; set; } // JSON array or comma separated
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? CustomScript { get; set; } // For complex rules
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int Priority { get; set; } = 0;
}

public class ComplianceCheck : BaseEntity
{
    public int AssetId { get; set; }
    public int? RuleId { get; set; }
    public DateTime CheckDate { get; set; }
    public string Status { get; set; } = string.Empty; // Compliant, NonCompliant, Warning, Pending
    public int Score { get; set; } // 0-100
    public string Findings { get; set; } = string.Empty; // JSON details of what was checked
    public string? Recommendations { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public string CheckedBy { get; set; } = string.Empty; // System or User ID
    public string? Evidence { get; set; } // JSON evidence references
    public bool IsAutomatic { get; set; } = true;
    
    // Navigation
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}

public class ComplianceHistory : BaseEntity
{
    public int AssetId { get; set; }
    public DateTime ChangeDate { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int FromScore { get; set; }
    public int ToScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; } // Rule ID, User ID, or System
    public string? Evidence { get; set; }
    
    // Navigation
    public virtual Asset? Asset { get; set; }
}

public class ComplianceAlert : BaseEntity
{
    public int AssetId { get; set; }
    public int? RuleId { get; set; }
    public string AlertType { get; set; } = string.Empty; // Warning, Violation, Expiry, Critical
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; } // 1-Low, 2-Medium, 3-High, 4-Critical
    public string Status { get; set; } = string.Empty; // New, Acknowledged, Resolved, Ignored
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? DueDate { get; set; }
    public bool RequiresAction { get; set; } = true;
    
    // Navigation
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}

public class ComplianceDashboard
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int WarningAssets { get; set; }
    public int CriticalAssets { get; set; }
    public double OverallComplianceRate { get; set; }
    public int ActiveAlerts { get; set; }
    public int OverdueActions { get; set; }
    public string? TopIssues { get; set; } // JSON
    public string? AssetTypeBreakdown { get; set; } // JSON
    public string? TrendData { get; set; } // JSON for charts
}