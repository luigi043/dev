using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace InsureX.Web.Controllers;

[Authorize]
public class PoliciesController : Controller
{
    private readonly IPolicyService _policyService;
    private readonly IAssetService _assetService;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(
        IPolicyService policyService,
        IAssetService assetService,
        ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _assetService = assetService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var search = new PolicySearchDto
        {
            Page = page,
            PageSize = 10
        };

        var result = await _policyService.GetPagedAsync(search);
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] PolicySearchDto search)
    {
        try
        {
            var result = await _policyService.GetPagedAsync(search);
            return PartialView("_PolicyTable", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching policies");
            return StatusCode(500, new { error = "Error searching policies" });
        }
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Assets = await GetAssetsList();
        return View(new CreatePolicyDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePolicyDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Assets = await GetAssetsList();
            return View(dto);
        }

        try
        {
            var policy = await _policyService.CreateAsync(dto);
            TempData["Success"] = $"Policy {policy.PolicyNumber} created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy");
            ModelState.AddModelError("", ex.Message);
            ViewBag.Assets = await GetAssetsList();
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var policy = await _policyService.GetByIdAsync(id);
        if (policy == null)
        {
            return NotFound();
        }

        ViewBag.Assets = await GetAssetsList(policy.AssetId);
        var dto = policy.Adapt<UpdatePolicyDto>();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdatePolicyDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Assets = await GetAssetsList(dto.AssetId);
            return View(dto);
        }

        try
        {
            var policy = await _policyService.UpdateAsync(dto);
            if (policy == null)
            {
                return NotFound();
            }

            TempData["Success"] = $"Policy {policy.PolicyNumber} updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy {Id}", id);
            ModelState.AddModelError("", ex.Message);
            ViewBag.Assets = await GetAssetsList(dto.AssetId);
            return View(dto);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _policyService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return Json(new { success = true, message = "Policy deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var policy = await _policyService.GetByIdAsync(id);
        if (policy == null)
        {
            return NotFound();
        }

        ViewBag.Claims = await _policyService.GetClaimsByPolicyIdAsync(id);
        return View(policy);
    }

    public async Task<IActionResult> Expiring()
    {
        var expiringPolicies = await _policyService.GetExpiringPoliciesAsync(30);
        return View(expiringPolicies);
    }

    public async Task<IActionResult> ByAsset(int assetId)
    {
        var asset = await _assetService.GetByIdAsync(assetId);
        if (asset == null)
        {
            return NotFound();
        }

        ViewBag.Asset = asset;
        var policies = await _policyService.GetByAssetIdAsync(assetId);
        return View(policies);
    }

    // Claims
    public async Task<IActionResult> Claims(int policyId)
    {
        var policy = await _policyService.GetByIdAsync(policyId);
        if (policy == null)
        {
            return NotFound();
        }

        ViewBag.Policy = policy;
        var claims = await _policyService.GetClaimsByPolicyIdAsync(policyId);
        return View(claims);
    }

    public async Task<IActionResult> AddClaim(int policyId)
    {
        var policy = await _policyService.GetByIdAsync(policyId);
        if (policy == null)
        {
            return NotFound();
        }

        ViewBag.Policy = policy;
        return View(new CreateClaimDto
        {
            PolicyId = policyId,
            ClaimDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaim(CreateClaimDto dto)
    {
        if (!ModelState.IsValid)
        {
            var policy = await _policyService.GetByIdAsync(dto.PolicyId);
            ViewBag.Policy = policy;
            return View(dto);
        }

        try
        {
            var claim = await _policyService.AddClaimAsync(dto);
            TempData["Success"] = "Claim submitted successfully!";
            return RedirectToAction(nameof(Details), new { id = dto.PolicyId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding claim");
            ModelState.AddModelError("", ex.Message);
            var policy = await _policyService.GetByIdAsync(dto.PolicyId);
            ViewBag.Policy = policy;
            return View(dto);
        }
    }

    private async Task<List<AssetDto>> GetAssetsList(int? selectedId = null)
    {
        var search = new AssetSearchDto
        {
            PageSize = 100,
            Status = "Active"
        };
        var assets = await _assetService.GetPagedAsync(search);
        return assets.Items;
    }
}