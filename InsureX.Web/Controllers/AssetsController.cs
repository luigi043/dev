using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Text;
using Mapster;

namespace InsureX.Web.Controllers;

[Authorize]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;
    private readonly IComplianceEngineService _complianceService;
    private readonly IPolicyService _policyService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(
        IAssetService assetService,
        IComplianceEngineService complianceService,
        IPolicyService policyService,
        ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _complianceService = complianceService;
        _policyService = policyService;
        _logger = logger;
    }

    // GET: Assets
    public async Task<IActionResult> Index(int page = 1, string? searchTerm = null)
    {
        var search = new AssetSearchDto
        {
            Page = page,
            SearchTerm = searchTerm,
            PageSize = 10
        };

        var result = await _assetService.GetPagedAsync(search);
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentSort = "assettag";
        ViewBag.CurrentSortDir = "asc";
        
        return View(result);
    }

    // POST: Assets/Search
    [HttpPost]
    public async Task<IActionResult> Search([FromBody] AssetSearchDto search)
    {
        try
        {
            if (search == null)
            {
                search = new AssetSearchDto
                {
                    Page = 1,
                    PageSize = 10
                };
            }

            var result = await _assetService.GetPagedAsync(search);
            
            // Set ViewBag for sorting
            ViewBag.CurrentSort = search.SortBy ?? "assettag";
            ViewBag.CurrentSortDir = search.SortDir ?? "asc";
            ViewBag.SearchTerm = search.SearchTerm;
            
            return PartialView("_AssetTable", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching assets");
            return StatusCode(500, new { error = "Error searching assets" });
        }
    }

    // GET: Assets/Create
    public IActionResult Create()
    {
        return View(new CreateAssetDto());
    }

    // POST: Assets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssetDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var asset = await _assetService.CreateAsync(dto);
            TempData["Success"] = $"Asset {asset.AssetTag} created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    // GET: Assets/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        var dto = new UpdateAssetDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Make = asset.Make,
            Model = asset.Model,
            Year = asset.Year,
            SerialNumber = asset.SerialNumber,
            VIN = asset.VIN,
            Status = asset.Status,
            ComplianceStatus = asset.ComplianceStatus,
            InsuredValue = asset.InsuredValue,
            PurchaseDate = asset.PurchaseDate,
            LastInspectionDate = asset.LastInspectionDate,
            Notes = asset.Notes,
            AssetType = asset.AssetType
        };
        
        return View(dto);
    }

    // POST: Assets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateAssetDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var asset = await _assetService.UpdateAsync(id, dto);
            if (asset == null)
            {
                return NotFound();
            }

            TempData["Success"] = $"Asset {asset.AssetTag} updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {Id}", id);
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    // POST: Assets/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _assetService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return Json(new { success = true, message = "Asset deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: Assets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

    // GET: Assets/QuickView/5
    public async Task<IActionResult> QuickView(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        return PartialView("_QuickView", asset);
    }

    // GET: Assets/Export
    public async Task<IActionResult> Export(AssetSearchDto search)
    {
        try
        {
            var excelBytes = await _assetService.ExportToExcelAsync(search);
            return File(
                excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Assets_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting assets");
            TempData["Error"] = "Error exporting assets";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Assets/GetStatistics
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var totalCount = await _assetService.GetTotalCountAsync();
            var statusDistribution = await _assetService.GetStatusDistributionAsync();
            
            // Get compliance rate
            var compliantCount = statusDistribution.GetValueOrDefault("Compliant", 0);
            var complianceRate = totalCount > 0 ? (compliantCount * 100 / totalCount) : 0;
            
            // Get total value (you'll need to add this to your service)
            var assets = await _assetService.GetPagedAsync(new AssetSearchDto { PageSize = 1000 });
            var totalValue = assets.Items.Sum(a => a.InsuredValue ?? 0);
            var avgValue = assets.Items.Any() ? totalValue / assets.Items.Count : 0;
            
            return Json(new
            {
                activeCount = statusDistribution.GetValueOrDefault("Active", 0),
                totalValue = totalValue,
                complianceRate = complianceRate,
                avgValue = avgValue,
                nonCompliant = statusDistribution.GetValueOrDefault("NonCompliant", 0)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting asset statistics");
            return Json(new { activeCount = 0, totalValue = 0, complianceRate = 0, avgValue = 0, nonCompliant = 0 });
        }
    }

    // POST: Assets/UpdateStatus
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        try
        {
            var asset = await _assetService.GetByIdAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            var updateDto = asset.Adapt<UpdateAssetDto>();
            updateDto.Status = status;
            
            await _assetService.UpdateAsync(id, updateDto);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for asset {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET: Assets/Compliance/5
    public async Task<IActionResult> Compliance(int id)
    {
        try
        {
            var compliance = await _complianceService.GetAssetComplianceStatusAsync(id);
            var history = await _complianceService.GetComplianceHistoryAsync(id, 30);
            var alerts = await _complianceService.GetActiveAlertsAsync(id);
            
            ViewBag.History = history;
            ViewBag.Alerts = alerts;
            
            return View(compliance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance for asset {Id}", id);
            return NotFound();
        }
    }

    // GET: Assets/Policies/5
    public async Task<IActionResult> Policies(int id)
    {
        try
        {
            var policies = await _policyService.GetByAssetIdAsync(id);
            return View(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for asset {Id}", id);
            return NotFound();
        }
    }

    // GET: Assets/Documents/5
    public async Task<IActionResult> Documents(int id)
    {
        // You'll need to implement document service
        // For now, return empty view
        return View();
    }

    // GET: Assets/AuditLog/5
    public async Task<IActionResult> AuditLog(int id)
    {
        // You'll need to implement audit service
        // For now, return empty view
        return View();
    }

    private string GenerateCsv(List<AssetDto> assets)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AssetTag,Make,Model,Year,VIN,Status,ComplianceStatus,InsuredValue,PurchaseDate");
        
        foreach (var asset in assets)
        {
            sb.AppendLine($"{asset.AssetTag},{asset.Make},{asset.Model},{asset.Year},{asset.VIN},{asset.Status},{asset.ComplianceStatus},{asset.InsuredValue},{asset.PurchaseDate:yyyy-MM-dd}");
        }
        
        return sb.ToString();
    }
}