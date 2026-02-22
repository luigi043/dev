using System.Collections.Generic;

namespace InsureX.Application.DTOs;

/// <summary>
/// Summary of compliance status across assets
/// </summary>
public class ComplianceSummaryDto
{
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
    /// Overall compliance rate
    /// </summary>
    public double ComplianceRate => TotalAssets > 0 
        ? (double)CompliantAssets / TotalAssets * 100 
        : 0;
    
    /// <summary>
    /// Count by severity level
    /// </summary>
    public Dictionary<ComplianceSeverity, int> SeverityCounts { get; set; } = new();
    
    /// <summary>
    /// Recent compliance checks (last 30 days)
    /// </summary>
    public int RecentChecksCount { get; set; }
    
    /// <summary>
    /// Overdue checks
    /// </summary>
    public int OverdueChecks { get; set; }
    
    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; }
}