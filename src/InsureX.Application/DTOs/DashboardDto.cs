namespace InsureX.Application.DTOs;

public class DashboardViewModel
{
    public int TotalAssets { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public double ComplianceRate { get; set; }
    public List<RecentAlertDto> RecentAlerts { get; set; } = new();
    public List<ActivityDto> RecentActivities { get; set; } = new();
    public Dictionary<string, int> AssetsByStatus { get; set; } = new();
}

public class RecentAlertDto
{
    public int Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

public class ActivityDto
{
    public string User { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}