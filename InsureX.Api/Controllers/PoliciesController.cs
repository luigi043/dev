using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;

namespace InsureX.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(
        IPolicyService policyService,
        ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _logger = logger;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PolicySummaryData>> GetSummary()
    {
        try
        {
            var summary = await _policyService.GetSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy summary");
            return StatusCode(500, new { error = "Error getting policy summary" });
        }
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<List<PolicyDto>>> GetExpiringPolicies([FromQuery] int days = 30)
    {
        try
        {
            var policies = await _policyService.GetExpiringPoliciesAsync(days);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring policies");
            return StatusCode(500, new { error = "Error getting expiring policies" });
        }
    }

    [HttpGet("asset/{assetId}")]
    public async Task<ActionResult<List<PolicyDto>>> GetByAssetId(int assetId)
    {
        try
        {
            var policies = await _policyService.GetByAssetIdAsync(assetId);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Error getting policies for asset" });
        }
    }
}
