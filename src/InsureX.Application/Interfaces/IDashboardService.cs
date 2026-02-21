using InsureX.Application.DTOs;
using InsureX.Domain.Interfaces;      // for repositories
using InsureX.Application.Interfaces; // for Notification/User/Tenant services
namespace InsureX.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
    Task<int> GetTotalAssetsCountAsync();
    Task<int> GetActivePoliciesCountAsync();
    Task<double> GetComplianceRateAsync();
    Task<List<RecentAlertDto>> GetRecentAlertsAsync(int count);
}