namespace InsureX.Application.DTOs;

public class DashboardViewModel
{
    public int TotalAssets { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public double ComplianceRate { get; set; }
    public List<RecentAlertDto> RecentAlerts { get; set; } = new();
    public List<ChartDataDto> AssetTypeBreakdown { get; set; } = new();
}

public class RecentAlertDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Severity { get; set; }
    public string SeverityText { get; set; } = string.Empty;
}

public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}