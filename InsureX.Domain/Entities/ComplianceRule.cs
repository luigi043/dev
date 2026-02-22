using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsureX.Domain.Entities;

/// <summary>
/// Represents a compliance rule that defines conditions for asset compliance
/// </summary>
public class ComplianceRule : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Display name of the compliance rule
    /// </summary>
    [Required]
    [StringLength(200)]
    public string RuleName { get; set; } = string.Empty;

    /// <summary>
    /// Unique code identifier for the rule
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the rule's purpose
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of rule: Policy, Payment, Inspection, Documentation, Expiry, Value
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// JSON condition that defines when the rule applies
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// JSON action to take when rule is violated
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Severity level: 1-Low, 2-Medium, 3-High, 4-Critical
    /// </summary>
    public int Severity { get; set; } = 1;

    /// <summary>
    /// Whether the rule is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Days before expiry to trigger warning (for expiry-based rules)
    /// </summary>
    public int? DaysToExpiry { get; set; }

    /// <summary>
    /// JSON array of applicable policy types
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicablePolicyTypes { get; set; }

    /// <summary>
    /// JSON array of applicable asset types
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? ApplicableAssetTypes { get; set; }

    /// <summary>
    /// Minimum value threshold for value-based rules
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Maximum value threshold for value-based rules
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Custom script for complex rule evaluation
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? CustomScript { get; set; }

    /// <summary>
    /// Date from which the rule becomes effective
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Date after which the rule expires
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Execution priority (higher numbers run first)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// When the rule was last evaluated
    /// </summary>
    public DateTime? LastEvaluatedAt { get; set; }

    /// <summary>
    /// Number of times this rule has been violated
    /// </summary>
    public int ViolationCount { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();
    public virtual ICollection<ComplianceAlert> Alerts { get; set; } = new List<ComplianceAlert>();
}

/// <summary>
/// Represents a compliance check performed on an asset
/// </summary>
public class ComplianceCheck : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// ID of the asset being checked
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// ID of the rule used for this check (null for manual checks)
    /// </summary>
    public Guid? RuleId { get; set; }

    /// <summary>
    /// When the check was performed
    /// </summary>
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Result status: Compliant, NonCompliant, Warning, Pending, Error
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Compliance score 0-100
    /// </summary>
    [Range(0, 100)]
    public int Score { get; set; }

    /// <summary>
    /// JSON details of what was checked and the results
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string Findings { get; set; } = string.Empty;

    /// <summary>
    /// Recommendations for remediation
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Recommendations { get; set; }

    /// <summary>
    /// When the next check should be performed
    /// </summary>
    public DateTime? NextCheckDate { get; set; }

    /// <summary>
    /// Who or what performed the check (System or User ID)
    /// </summary>
    [StringLength(100)]
    public string CheckedBy { get; set; } = "System";

    /// <summary>
    /// JSON references to evidence documents
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Evidence { get; set; }

    /// <summary>
    /// Whether the check was automatic or manual
    /// </summary>
    public bool IsAutomatic { get; set; } = true;

    /// <summary>
    /// Time taken to perform the check in milliseconds
    /// </summary>
    public int? DurationMs { get; set; }

    /// <summary>
    /// Any error message if the check failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
}

/// <summary>
/// Tracks historical changes in compliance status
/// </summary>
public class ComplianceHistory : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// ID of the asset whose status changed
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// When the change occurred
    /// </summary>
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous compliance status
    /// </summary>
    [Required]
    [StringLength(50)]
    public string FromStatus { get; set; } = string.Empty;

    /// <summary>
    /// New compliance status
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ToStatus { get; set; } = string.Empty;

    /// <summary>
    /// Previous compliance score
    /// </summary>
    [Range(0, 100)]
    public int FromScore { get; set; }

    /// <summary>
    /// New compliance score
    /// </summary>
    [Range(0, 100)]
    public int ToScore { get; set; }

    /// <summary>
    /// Reason for the status change
    /// </summary>
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// What triggered the change (Rule ID, User ID, or System)
    /// </summary>
    [StringLength(100)]
    public string? TriggeredBy { get; set; }

    /// <summary>
    /// JSON evidence supporting the change
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Evidence { get; set; }

    /// <summary>
    /// ID of the related compliance check (if any)
    /// </summary>
    public Guid? ComplianceCheckId { get; set; }

    /// <summary>
    /// ID of the related alert (if any)
    /// </summary>
    public Guid? AlertId { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceCheck? ComplianceCheck { get; set; }
    public virtual ComplianceAlert? Alert { get; set; }
}

/// <summary>
/// Represents an alert generated by compliance violations
/// </summary>
public class ComplianceAlert : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// ID of the asset triggering the alert
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// ID of the rule that triggered the alert
    /// </summary>
    public Guid? RuleId { get; set; }

    /// <summary>
    /// ID of the compliance check that generated this alert
    /// </summary>
    public Guid? ComplianceCheckId { get; set; }

    /// <summary>
    /// Type of alert: Warning, Violation, Expiry, Critical, Info
    /// </summary>
    [Required]
    [StringLength(50)]
    public string AlertType { get; set; } = string.Empty;

    /// <summary>
    /// Short alert title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed alert description
    /// </summary>
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity: 1-Low, 2-Medium, 3-High, 4-Critical
    /// </summary>
    public int Severity { get; set; } = 1;

    /// <summary>
    /// Alert status: New, Acknowledged, InProgress, Resolved, Ignored, Expired
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";

    /// <summary>
    /// When the alert was acknowledged
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Who acknowledged the alert (User ID)
    /// </summary>
    [StringLength(100)]
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// When the alert was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Who resolved the alert (User ID)
    /// </summary>
    [StringLength(100)]
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// Notes about how the alert was resolved
    /// </summary>
    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Date by which action must be taken
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Whether this alert requires action
    /// </summary>
    public bool RequiresAction { get; set; } = true;

    /// <summary>
    /// Priority for handling: 1-Highest, 5-Lowest
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Number of times this alert has been escalated
    /// </summary>
    public int EscalationCount { get; set; }

    /// <summary>
    /// ID of the parent alert if this is a follow-up
    /// </summary>
    public Guid? ParentAlertId { get; set; }

    /// <summary>
    /// JSON data with additional context
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual Asset? Asset { get; set; }
    public virtual ComplianceRule? Rule { get; set; }
    public virtual ComplianceCheck? ComplianceCheck { get; set; }
    public virtual ComplianceAlert? ParentAlert { get; set; }
    public virtual ICollection<ComplianceAlert> ChildAlerts { get; set; } = new List<ComplianceAlert>();
}

/// <summary>
/// Dashboard snapshot for compliance metrics
/// </summary>
public class ComplianceDashboard : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// When this snapshot was taken
    /// </summary>
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of assets
    /// </summary>
    public int TotalAssets { get; set; }

    /// <summary>
    /// Number of compliant assets
    /// </summary>
    public int CompliantAssets { get; set; }

    /// <summary>
    /// Number of non-compliant assets
    /// </summary>
    public int NonCompliantAssets { get; set; }

    /// <summary>
    /// Number of assets with warnings
    /// </summary>
    public int WarningAssets { get; set; }

    /// <summary>
    /// Number of assets with critical issues
    /// </summary>
    public int CriticalAssets { get; set; }

    /// <summary>
    /// Assets pending review
    /// </summary>
    public int PendingReviewAssets { get; set; }

    /// <summary>
    /// Overall compliance rate percentage
    /// </summary>
    [Range(0, 100)]
    public double OverallComplianceRate { get; set; }

    /// <summary>
    /// Number of active alerts
    /// </summary>
    public int ActiveAlerts { get; set; }

    /// <summary>
    /// Number of overdue actions
    /// </summary>
    public int OverdueActions { get; set; }

    /// <summary>
    /// Number of actions due soon (next 7 days)
    /// </summary>
    public int ActionsDueSoon { get; set; }

    /// <summary>
    /// Average response time to alerts in hours
    /// </summary>
    public double AverageResponseTimeHours { get; set; }

    /// <summary>
    /// Average resolution time in hours
    /// </summary>
    public double AverageResolutionTimeHours { get; set; }

    /// <summary>
    /// JSON data of top issues by frequency
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? TopIssues { get; set; }

    /// <summary>
    /// JSON breakdown by asset type
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? AssetTypeBreakdown { get; set; }

    /// <summary>
    /// JSON breakdown by rule type
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? RuleTypeBreakdown { get; set; }

    /// <summary>
    /// JSON trend data for charts
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? TrendData { get; set; }

    /// <summary>
    /// JSON data for alerts by severity
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? AlertsBySeverity { get; set; }

    /// <summary>
    /// Whether this is the current dashboard
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Notes about this snapshot
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
}