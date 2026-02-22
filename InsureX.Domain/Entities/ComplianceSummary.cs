namespace InsureX.Domain.Entities;

public class ComplianceSummary
{
    public int TotalAssets { get; set; }
    public int CompliantCount { get; set; }
    public int NonCompliantCount { get; set; }
    public int PendingCount { get; set; }
    public double CompliancePercentage { get; set; }
    public Dictionary<string, int> ByAssetType { get; set; } = new();
    public Dictionary<string, int> BySeverity { get; set; } = new();
}