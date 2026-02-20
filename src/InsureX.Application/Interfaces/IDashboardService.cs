using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
    Task<int> GetTotalAssetsCountAsync();
    Task<int> GetActivePoliciesCountAsync();
    Task<double> GetComplianceRateAsync();
    Task<List<RecentAlertDto>> GetRecentAlertsAsync(int count);
}