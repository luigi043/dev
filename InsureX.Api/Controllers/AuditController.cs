using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InsureX.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly ILogger<AuditController> _logger;

    public AuditController(ILogger<AuditController> logger)
    {
        _logger = logger;
    }

    [HttpGet("assets/{assetId}")]
    public async Task<IActionResult> GetAssetAuditLog(int assetId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            // Return empty result for now - implement your audit service
            var result = new
            {
                items = new List<object>(),
                page = page,
                pageSize = pageSize,
                totalItems = 0,
                totalPages = 0
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Error retrieving audit log" });
        }
    }
}