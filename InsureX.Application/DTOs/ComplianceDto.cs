using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

/// <summary>
/// Compliance severity levels
/// </summary>
public enum ComplianceSeverity
{
    Info = 0,
    Minor = 1,
    Warning = 2,
    Critical = 3,
    Blocker = 4
}

/// <summary>
/// Represents a compliance rule
/// </summary>
public class ComplianceRuleDto
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsActive { get; set; }
    public int? DaysToExpiry { get; set; }
    public int Priority { get; set; }
    public string? ApplicableAssetTypes { get; set; }
}

/// <summary>
/// DTO for creating a new compliance rule
/// </summary>
public class CreateComplianceRuleDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public int Severity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int? DaysToExpiry { get; set; }
    public int Priority { get; set; } = 0;
    public string? ApplicableAssetTypes { get; set; }
}

/// <summary>
/// Represents a compliance alert for an asset
/// </summary>
public class ComplianceAlertDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public int? RuleId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Result of a compliance check for an asset
/// </summary>
public class ComplianceCheckResultDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<string> Violations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Details { get; set; }
    public string? Recommendations { get; set; }
    public DateTime CheckedAt { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public string CheckedBy { get; set; } = string.Empty;
    public ComplianceSeverity Severity { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
}

/// <summary>
/// Summary of a compliance check (for history lists)
/// </summary>
public class ComplianceCheckDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public string CheckedBy { get; set; } = string.Empty;
}

/// <summary>
/// History of compliance status changes for an asset
/// </summary>
public class ComplianceHistoryDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? ComplianceCheckId { get; set; }
}

/// <summary>
/// Current compliance status of a single asset
/// </summary>
public class ComplianceStatusDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime LastChecked { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
}

/// <summary>
/// Summary of compliance status across all assets
/// </summary>
public class ComplianceSummaryDto
{
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int WarningAssets { get; set; }
    public double ComplianceRate => TotalAssets > 0 ? (double)CompliantAssets / TotalAssets * 100 : 0;
    public int ActiveAlerts { get; set; }
    public Dictionary<ComplianceSeverity, int> SeverityCounts { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Dashboard data for compliance overview
/// </summary>
public class ComplianceDashboardDto
{
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int WarningAssets { get; set; }
    public double OverallComplianceRate => TotalAssets > 0 ? (double)CompliantAssets / TotalAssets * 100 : 0;
    public int ActiveAlerts { get; set; }
    public List<ComplianceAlertDto> RecentAlerts { get; set; } = new();
    public List<TopIssueDto> TopIssues { get; set; } = new();
}

/// <summary>
/// Top compliance issues for reporting
/// </summary>
public class TopIssueDto
{
    public string Issue { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Severity { get; set; }
}