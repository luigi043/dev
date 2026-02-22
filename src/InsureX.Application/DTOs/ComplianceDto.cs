using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

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
    public int Priority { get; set; }
}

public class ComplianceAlertDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ComplianceStatusDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime LastChecked { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
}

public class ComplianceDashboardDto
{
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int WarningAssets { get; set; }
    public double OverallComplianceRate { get; set; }
    public int ActiveAlerts { get; set; }
    public List<ComplianceAlertDto> RecentAlerts { get; set; } = new();
    public List<TopIssueDto> TopIssues { get; set; } = new();
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
    public int Priority { get; set; } = 0;
}

public class TopIssueDto
{
    public string Issue { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Severity { get; set; }
}