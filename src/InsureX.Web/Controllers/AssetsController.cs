using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;

namespace InsureX.Web.Controllers;

[Authorize]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 25)
    {
        var assets = await _assetService.GetPagedAsync(page, pageSize);
        return View(assets);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssetDto dto)
    {
        if (ModelState.IsValid)
        {
            await _assetService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Search(string searchTerm, int page = 1)
    {
        var results = await _assetService.SearchAsync(searchTerm, page);
        return PartialView("_AssetTable", results);
    }
}