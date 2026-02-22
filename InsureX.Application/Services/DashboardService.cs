using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;

namespace InsureX.Application.Services;

public class DashboardService : IDashboardService
{
    public Task<DashboardViewModel> GetSummaryAsync()
    {
        var summary = new DashboardViewModel
        {
            TotalAssets = 0,
            ActivePolicies = 0,
            ExpiringPolicies = 0,
            ComplianceRate = 0.0
        };
        return Task.FromResult(summary);
    }

    public Task<DashboardViewModel> GetDashboardDataAsync()
    {
        return Task.FromResult(new DashboardViewModel());
    }

    public Task<int> GetTotalAssetsCountAsync() => Task.FromResult(0);
    public Task<int> GetActivePoliciesCountAsync() => Task.FromResult(0);
    public Task<double> GetComplianceRateAsync() => Task.FromResult(0.0);
    public Task<List<RecentAlertDto>> GetRecentAlertsAsync(int count) => Task.FromResult(new List<RecentAlertDto>());
}