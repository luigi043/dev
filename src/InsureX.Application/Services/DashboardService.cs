using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsureX.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IAssetRepository assetRepository,
        IPolicyRepository policyRepository,
        IAuditRepository auditRepository,
        ILogger<DashboardService> logger)
    {
        _assetRepository = assetRepository;
        _policyRepository = policyRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        try
        {
            var viewModel = new DashboardViewModel
            {
                TotalAssets = await GetTotalAssetsCountAsync(),
                ActivePolicies = await GetActivePoliciesCountAsync(),
                ExpiringPolicies = await _policyRepository.GetExpiringCountAsync(30), // Next 30 days
                ComplianceRate = await GetComplianceRateAsync(),
                RecentAlerts = await GetRecentAlertsAsync(10),
                RecentActivities = await GetRecentActivitiesAsync(5),
                AssetsByStatus = await GetAssetsByStatusAsync()
            };

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            throw;
        }
    }

    public async Task<int> GetTotalAssetsCountAsync()
    {
        return await _assetRepository.GetCountAsync();
    }

    public async Task<int> GetActivePoliciesCountAsync()
    {
        return await _policyRepository.GetActiveCountAsync();
    }

    public async Task<double> GetComplianceRateAsync()
    {
        var totalAssets = await _assetRepository.GetCountAsync();
        if (totalAssets == 0) return 0;

        var compliantAssets = await _assetRepository.GetCompliantCountAsync();
        return Math.Round((double)compliantAssets / totalAssets * 100, 2);
    }

    public async Task<List<RecentAlertDto>> GetRecentAlertsAsync(int count)
    {
        var alerts = await _auditRepository.GetRecentAlertsAsync(count);
        return alerts.Select(a => new RecentAlertDto
        {
            Id = a.Id,
            AssetName = a.Asset?.AssetTag ?? "Unknown",
            AlertType = a.Action,
            Date = a.CreatedAt,
            Status = a.Status,
            Severity = GetSeverity(a.Action)
        }).ToList();
    }

    private async Task<List<ActivityDto>> GetRecentActivitiesAsync(int count)
    {
        var activities = await _auditRepository.GetRecentActivitiesAsync(count);
        return activities.Select(a => new ActivityDto
        {
            User = a.User?.UserName ?? "System",
            Action = a.Action,
            Entity = a.EntityType,
            Timestamp = a.CreatedAt
        }).ToList();
    }

    private async Task<Dictionary<string, int>> GetAssetsByStatusAsync()
    {
        return await _assetRepository.GetCountByStatusAsync();
    }

    private string GetSeverity(string action)
    {
        return action.ToLower() switch
        {
            var a when a.Contains("expire") => "warning",
            var a when a.Contains("violation") => "danger",
            var a when a.Contains("compliance") => "info",
            _ => "secondary"
        };
    }
}