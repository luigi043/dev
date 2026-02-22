using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureX.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InsureX.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(ILogger<DocumentsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("assets/{assetId}")]
    public async Task<IActionResult> GetAssetDocuments(int assetId)
    {
        try
        {
            // Return empty list for now - implement your document service
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Error retrieving documents" });
        }
    }

    [HttpPost("assets/{assetId}")]
    public async Task<IActionResult> UploadAssetDocument(int assetId, [FromForm] DocumentUploadDto dto)
    {
        try
        {
            // Implement document upload logic
            return Ok(new { success = true, message = "Document uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for asset {AssetId}", assetId);
            return StatusCode(500, new { error = "Error uploading document" });
        }
    }
}

public class DocumentUploadDto
{
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile? File { get; set; }
}