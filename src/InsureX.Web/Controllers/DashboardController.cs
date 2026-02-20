using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;

namespace InsureX.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IPolicyService _policyService;
    private readonly IComplianceService _complianceService;

    public DashboardController(
        IAssetService assetService,
        IPolicyService policyService,
        IComplianceService complianceService)
    {
        _assetService = assetService;
        _policyService = policyService;
        _complianceService = complianceService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new DashboardViewModel
        {
            TotalAssets = await _assetService.GetCountAsync(),
            ActivePolicies = await _policyService.GetActiveCountAsync(),
            ComplianceRate = await _complianceService.GetComplianceRateAsync(),
            RecentAlerts = await _complianceService.GetRecentAlertsAsync(5)
        };
        
        return View(viewModel);
    }
}

public class DashboardViewModel
{
    public int TotalAssets { get; set; }
    public int ActivePolicies { get; set; }
    public double ComplianceRate { get; set; }
    public List<AlertDto> RecentAlerts { get; set; }
}