using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InsureX.Web.Controllers;

[Authorize]
public class ComplianceController : Controller
{
    private readonly IComplianceEngineService _complianceService;
    private readonly IAssetService _assetService;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(
        IComplianceEngineService complianceService,
        IAssetService assetService,
        ILogger<ComplianceController> logger)
    {
        _complianceService = complianceService;
        _assetService = assetService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _complianceService.GetDashboardDataAsync();
        return View(dashboard);
    }

    public async Task<IActionResult> Assets()
    {
        var nonCompliant = await _complianceService.GetNonCompliantAssetsReportAsync();
        return View(nonCompliant);
    }

    public async Task<IActionResult> AssetDetails(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        var status = await _complianceService.GetAssetComplianceStatusAsync(id);
        var history = await _complianceService.GetComplianceHistoryAsync(id);
        var alerts = await _complianceService.GetActiveAlertsAsync(id);

        ViewBag.Asset = asset;
        ViewBag.History = history;
        ViewBag.Alerts = alerts;
        
        return View(status);
    }

    public async Task<IActionResult> Alerts()
    {
        var alerts = await _complianceService.GetActiveAlertsAsync();
        return View(alerts);
    }

    [HttpPost]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var alert = await _complianceService.AcknowledgeAlertAsync(id, userId);
            return Json(new { success = true, message = "Alert acknowledged" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging alert {AlertId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ResolveAlert(int id, string notes)
    {
        try
        {
            var userId = User.Identity?.Name ?? "system";
            var alert = await _complianceService.ResolveAlertAsync(id, userId, notes);
            return Json(new { success = true, message = "Alert resolved" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RunComplianceCheck(int? assetId = null)
    {
        try
        {
            if (assetId.HasValue)
            {
                var result = await _complianceService.CheckAssetComplianceAsync(assetId.Value);
                return Json(new { success = true, message = $"Compliance check completed: {result.Status}" });
            }
            else
            {
                var results = await _complianceService.CheckAllAssetsAsync();
                return Json(new { success = true, message = $"Compliance check completed for {results.Count} assets" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running compliance check");
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Rules()
    {
        var rules = await _complianceService.GetActiveRulesAsync();
        return View(rules);
    }

    public IActionResult CreateRule()
    {
        return View(new CreateComplianceRuleDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRule(CreateComplianceRuleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var rule = await _complianceService.CreateRuleAsync(dto);
            TempData["Success"] = $"Rule {rule.RuleName} created successfully!";
            return RedirectToAction(nameof(Rules));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance rule");
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> ExportReport(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var reportData = await _complianceService.GenerateComplianceReportAsync(fromDate, toDate);
            return File(
                reportData,
                "text/csv",
                $"Compliance_Report_{DateTime.Now:yyyyMMdd}.csv"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance report");
            TempData["Error"] = "Error generating report";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> RefreshDashboard()
    {
        try
        {
            var dashboard = await _complianceService.RefreshDashboardAsync();
            TempData["Success"] = "Dashboard refreshed successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing dashboard");
            TempData["Error"] = "Error refreshing dashboard";
            return RedirectToAction(nameof(Index));
        }
    }
}