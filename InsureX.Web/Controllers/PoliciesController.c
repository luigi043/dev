using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.DTOs;
using InsureX.Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    // GET: Policies
    public async Task<IActionResult> Index(PolicySearchDto search)
    {
        try
        {
            if (search == null)
            {
                search = new PolicySearchDto
                {
                    Page = 1,
                    PageSize = 25,
                    SortBy = "enddate",
                    SortDir = "asc"
                };
            }

            var result = await _policyService.GetPagedAsync(search);
            
            // Store search parameters in ViewBag for form persistence
            ViewBag.SearchTerm = search.SearchTerm;
            ViewBag.Status = search.Status;
            ViewBag.ExpiringOnly = search.ExpiringOnly;
            
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Policies Index");
            TempData["Error"] = "An error occurred while loading policies.";
            return View(new PagedResult<PolicyDto>());
        }
    }

    // POST: Policies/Search (for AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search([FromBody] PolicySearchDto search)
    {
        try
        {
            var result = await _policyService.GetPagedAsync(search);
            return PartialView("_PolicyTable", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in policy search");
            return Json(new { error = "Search failed" });
        }
    }

    // GET: Policies/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }
            return View(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy details for {PolicyId}", id);
            TempData["Error"] = "Error loading policy details.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Policies/Create
    public async Task<IActionResult> Create(int? assetId = null)
    {
        try
        {
            var dto = new CreatePolicyDto
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                AssetId = assetId ?? 0
            };

            await PopulateDropdowns(dto.AssetId);
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create policy form");
            TempData["Error"] = "Error loading form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Policies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePolicyDto dto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var policy = await _policyService.CreateAsync(dto);
                TempData["Success"] = $"Policy {policy.PolicyNumber} created successfully!";
                
                // Redirect to asset details if coming from asset
                if (dto.AssetId > 0)
                {
                    return RedirectToAction("Details", "Assets", new { id = dto.AssetId });
                }
                
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy");
            ModelState.AddModelError("", ex.Message);
        }

        await PopulateDropdowns(dto.AssetId);
        return View(dto);
    }

    // GET: Policies/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var policy = await _policyService.GetByIdAsync(id);
            if (policy == null)
            {
                return NotFound();
            }

            var dto = policy.Adapt<UpdatePolicyDto>();
            await PopulateDropdowns(dto.AssetId);
            
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit policy form for {PolicyId}", id);
            TempData["Error"] = "Error loading form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Policies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdatePolicyDto dto)
    {
        if (id != dto.Id)
        {
            return NotFound();
        }

        try
        {
            if (ModelState.IsValid)
            {
                await _policyService.UpdateAsync(dto);
                TempData["Success"] = "Policy updated successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy {PolicyId}", id);
            ModelState.AddModelError("", ex.Message);
        }

        await PopulateDropdowns(dto.AssetId);
        return View(dto);
    }

    // POST: Policies/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _policyService.DeleteAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Policy deleted successfully." });
            }
            return Json(new { success = false, message = "Policy not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", id);
            return Json(new { success = false, message = "Error deleting policy." });
        }
    }

    // GET: Policies/ByAsset/5
    public async Task<IActionResult> ByAsset(int assetId)
    {
        try
        {
            var policies = await _policyService.GetByAssetIdAsync(assetId);
            return PartialView("_AssetPolicies", policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for asset {AssetId}", assetId);
            return PartialView("_AssetPolicies", new List<PolicyDto>());
        }
    }

    // POST: Policies/ProcessPayment/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentDto payment)
    {
        try
        {
            var result = await _policyService.ProcessPaymentAsync(id, payment);
            if (result)
            {
                return Json(new { success = true, message = "Payment processed successfully." });
            }
            return Json(new { success = false, message = "Policy not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for policy {PolicyId}", id);
            return Json(new { success = false, message = "Error processing payment." });
        }
    }

    // POST: Policies/AddClaim
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClaim([FromBody] CreateClaimDto claim)
    {
        try
        {
            var result = await _policyService.AddClaimAsync(claim);
            return Json(new { success = true, message = "Claim submitted successfully.", claim = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding claim");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: Policies/Export
    public async Task<IActionResult> Export(PolicySearchDto search)
    {
        try
        {
            var csvData = await _policyService.ExportPoliciesAsync(search);
            return File(csvData, "text/csv", $"policies_export_{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting policies");
            TempData["Error"] = "Error exporting policies.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Policies/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var summary = await _policyService.GetSummaryAsync();
            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading policy dashboard");
            TempData["Error"] = "Error loading dashboard.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ======= Private Helper Methods =======

    private async Task PopulateDropdowns(int selectedAssetId = 0)
    {
        var assets = await _assetService.GetAllAsync();
        ViewBag.Assets = new SelectList(assets, "Id", "AssetTag", selectedAssetId);
        
        ViewBag.PolicyTypes = new SelectList(new[]
        {
            new { Value = "Auto", Text = "Auto Insurance" },
            new { Value = "Home", Text = "Home Insurance" },
            new { Value = "Life", Text = "Life Insurance" },
            new { Value = "Health", Text = "Health Insurance" },
            new { Value = "Business", Text = "Business Insurance" },
            new { Value = "Equipment", Text = "Equipment Insurance" }
        }, "Value", "Text");
    }
}