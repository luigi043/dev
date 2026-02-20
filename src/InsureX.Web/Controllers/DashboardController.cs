using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboardData = await _dashboardService.GetDashboardDataAsync();
            return View(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            TempData["Error"] = "Unable to load dashboard data. Please try again.";
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = new
        {
            totalAssets = await _dashboardService.GetTotalAssetsCountAsync(),
            activePolicies = await _dashboardService.GetActivePoliciesCountAsync(),
            complianceRate = await _dashboardService.GetComplianceRateAsync(),
            recentAlerts = await _dashboardService.GetRecentAlertsAsync(5)
        };
        return Json(stats);
    }
}
public class DashboardViewModel
{
    public int TotalAssets { get; set; }
    public int ActivePolicies { get; set; }
    public double ComplianceRate { get; set; }
    public List<AlertDto> RecentAlerts { get; set; }
}