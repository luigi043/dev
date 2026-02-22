using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class ComplianceDashboard : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int PendingChecks { get; set; }
    public int ActiveAlerts { get; set; }
    public double OverallComplianceRate { get; set; }
    public double AverageScore { get; set; }
    public string ComplianceByTypeJson { get; set; } = "{}";
    public string AlertsBySeverityJson { get; set; } = "{}";
    public string TopViolationsJson { get; set; } = "{}";
    public string RecentActivityJson { get; set; } = "[]";
    
    // Navigation property
    public virtual Tenant Tenant { get; set; }
}