using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace InsureX.Web.Controllers;

[Authorize]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(
        IAssetService assetService,
        ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

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
        
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] AssetSearchDto search)
    {
        try
        {
            var result = await _assetService.GetPagedAsync(search);
            return PartialView("_AssetTable", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching assets");
            return StatusCode(500, new { error = "Error searching assets" });
        }
    }

    public IActionResult Create()
    {
        return View(new CreateAssetDto());
    }

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

    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        var dto = asset.Adapt<UpdateAssetDto>();
        return View(dto);
    }

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
            var asset = await _assetService.UpdateAsync(dto);
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

    [HttpPost]
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

    public async Task<IActionResult> Details(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
        {
            return NotFound();
        }

        return View(asset);
    }

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
}