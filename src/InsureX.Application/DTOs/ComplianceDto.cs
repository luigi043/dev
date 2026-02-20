using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

public class ComplianceStatusDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime LastChecked { get; set; }
    public DateTime? NextCheck { get; set; }
    public List<ComplianceFindingDto> Findings { get; set; } = new();
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
}

public class ComplianceFindingDto
{
    public string RuleName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Finding { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public int Severity { get; set; }
}

public class ComplianceRuleDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string RuleCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public int Severity { get; set; }
    public bool IsActive { get; set; }
    public int? DaysToExpiry { get; set; }
    public string[] ApplicablePolicyTypes { get; set; } = Array.Empty<string>();
    public string[] ApplicableAssetTypes { get; set; } = Array.Empty<string>();
    public int Priority { get; set; }
}

public class CreateComplianceRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int Severity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int? DaysToExpiry { get; set; }
    public string[] ApplicablePolicyTypes { get; set; } = Array.Empty<string>();
    public string[] ApplicableAssetTypes { get; set; } = Array.Empty<string>();
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public int Priority { get; set; } = 0;
}

public class ComplianceAlertDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public bool RequiresAction { get; set; }
}

public class ComplianceDashboardDto
{
    public DateTime AsOfDate { get; set; }
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int WarningAssets { get; set; }
    public int CriticalAssets { get; set; }
    public double OverallComplianceRate { get; set; }
    public int ActiveAlerts { get; set; }
    public int OverdueActions { get; set; }
    public List<ChartDataDto> AssetTypeBreakdown { get; set; } = new();
    public List<ChartDataDto> TrendData { get; set; } = new();
    public List<TopIssueDto> TopIssues { get; set; } = new();
    public List<ComplianceAlertDto> RecentAlerts { get; set; } = new();
}

public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string? Color { get; set; }
}

public class TopIssueDto
{
    public string Issue { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Severity { get; set; }
    public string Trend { get; set; } = "stable"; // up, down, stable
}

public class ComplianceCheckResult
{
    public int AssetId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<RuleEvaluationResult> RuleResults { get; set; } = new();
    public List<ComplianceAlertDto> NewAlerts { get; set; } = new();
}

public class RuleEvaluationResult
{
    public int RuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Finding { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
    public int Severity { get; set; }
}