using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

/// <summary>
/// Represents the result of a compliance check for an asset
/// </summary>
public class ComplianceCheckResultDto
{
    /// <summary>
    /// Unique identifier of the asset
    /// </summary>
    public Guid AssetId { get; set; } // Changed from int to Guid to match domain entity
    
    /// <summary>
    /// Asset tag or reference number
    /// </summary>
    public string AssetTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Current compliance status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the asset is compliant
    /// </summary>
    public bool IsCompliant { get; set; }
    
    /// <summary>
    /// Reason for non-compliance if applicable
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Compliance score (0-100)
    /// </summary>
    public int Score { get; set; }
    
    /// <summary>
    /// When the check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }
    
    /// <summary>
    /// Detailed findings from the compliance check
    /// </summary>
    public string Findings { get; set; } = string.Empty;
    
    /// <summary>
    /// Recommendations for improvement
    /// </summary>
    public string? Recommendations { get; set; }
    
    /// <summary>
    /// When the next compliance check is due
    /// </summary>
    public DateTime? NextCheckDate { get; set; }
    
    /// <summary>
    /// Active alerts for this asset
    /// </summary>
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
    
    /// <summary>
    /// Severity level of the compliance status
    /// </summary>
    public ComplianceSeverity Severity { get; set; }
    
    /// <summary>
    /// Time until next check (calculated property)
    /// </summary>
    public TimeSpan? TimeUntilNextCheck => NextCheckDate?.Subtract(DateTime.UtcNow);
    
    /// <summary>
    /// Whether the check is overdue
    /// </summary>
    public bool IsOverdue => NextCheckDate.HasValue && NextCheckDate.Value < DateTime.UtcNow;
}

/// <summary>
/// Represents a compliance alert for an asset
/// </summary>
public class ComplianceAlertDto
{
    /// <summary>
    /// Alert ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Alert type/category
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Alert severity
    /// </summary>
    public ComplianceSeverity Severity { get; set; }
    
    /// <summary>
    /// Alert message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// When the alert was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Whether the alert is resolved
    /// </summary>
    public bool IsResolved { get; set; }
    
    /// <summary>
    /// When the alert was resolved (if applicable)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Compliance severity levels
/// </summary>
public enum ComplianceSeverity
{
    /// <summary>
    /// Information only, no action needed
    /// </summary>
    Info = 0,
    
    /// <summary>
    /// Minor issue, should be monitored
    /// </summary>
    Warning = 1,
    
    /// <summary>
    /// Non-critical issue, needs attention
    /// </summary>
    Minor = 2,
    
    /// <summary>
    /// Critical issue, immediate action required
    /// </summary>
    Critical = 3,
    
    /// <summary>
    /// Severe breach, urgent intervention needed
    /// </summary>
    Blocker = 4
}namespace InsureX.Application.DTOs;

public class ComplianceCheckResultDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Violations { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public DateTime CheckedAt { get; set; }
    public int Score { get; set; }
}

public class ComplianceHistoryDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public int Score { get; set; }
}

public class ComplianceCheckDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string CheckType { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Details { get; set; }
}