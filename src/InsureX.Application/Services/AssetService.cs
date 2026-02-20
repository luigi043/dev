using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Exceptions;
using Mapster;

namespace InsureX.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AssetService> _logger;

    public AssetService(
        IAssetRepository assetRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<AssetService> logger)
    {
        _assetRepository = assetRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<AssetDto>> GetPagedAsync(AssetSearchDto search)
    {
        try
        {
            var query = await _assetRepository.GetQueryableAsync();
            
            // Apply search filters
            if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            {
                query = query.Where(a => 
                    a.AssetTag.Contains(search.SearchTerm) ||
                    a.Make.Contains(search.SearchTerm) ||
                    a.Model.Contains(search.SearchTerm) ||
                    a.SerialNumber.Contains(search.SearchTerm) ||
                    a.VIN.Contains(search.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(a => a.Status == search.Status);
            }

            if (!string.IsNullOrWhiteSpace(search.ComplianceStatus))
            {
                query = query.Where(a => a.ComplianceStatus == search.ComplianceStatus);
            }

            if (search.Year.HasValue)
            {
                query = query.Where(a => a.Year == search.Year.Value);
            }

            if (search.FromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= search.FromDate.Value);
            }

            if (search.ToDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= search.ToDate.Value);
            }

            // Apply sorting
            query = search.SortBy?.ToLower() switch
            {
                "assettag" => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.AssetTag) 
                    : query.OrderByDescending(a => a.AssetTag),
                "make" => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.Make) 
                    : query.OrderByDescending(a => a.Make),
                "model" => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.Model) 
                    : query.OrderByDescending(a => a.Model),
                "year" => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.Year) 
                    : query.OrderByDescending(a => a.Year),
                "status" => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.Status) 
                    : query.OrderByDescending(a => a.Status),
                _ => search.SortDir == "asc" 
                    ? query.OrderBy(a => a.CreatedAt) 
                    : query.OrderByDescending(a => a.CreatedAt)
            };

            var totalItems = await _assetRepository.CountAsync(query);
            var items = await _assetRepository.GetPagedAsync(query, search.Page, search.PageSize);

            return new PagedResult<AssetDto>
            {
                Items = items.Adapt<List<AssetDto>>(),
                Page = search.Page,
                PageSize = search.PageSize,
                TotalItems = totalItems
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged assets");
            throw;
        }
    }

    public async Task<AssetDto?> GetByIdAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        return asset?.Adapt<AssetDto>();
    }

    public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
    {
        // Check for duplicate asset tag
        if (await ExistsAsync(dto.AssetTag))
        {
            throw new DomainException($"Asset with tag {dto.AssetTag} already exists");
        }

        var asset = dto.Adapt<Asset>();
        asset.TenantId = _tenantContext.GetCurrentTenantId() 
            ?? throw new UnauthorizedAccessException("No tenant context");
        asset.CreatedBy = _currentUserService.GetCurrentUserId() ?? "system";
        asset.CreatedAt = DateTime.UtcNow;
        asset.Status = "Active";
        asset.ComplianceStatus = "Pending";

        await _assetRepository.AddAsync(asset);
        await _assetRepository.SaveChangesAsync();

        _logger.LogInformation("Asset created: {AssetTag} by {User}", asset.AssetTag, asset.CreatedBy);

        return asset.Adapt<AssetDto>();
    }

    public async Task<AssetDto?> UpdateAsync(UpdateAssetDto dto)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.Id);
        if (asset == null)
        {
            throw new DomainException($"Asset with id {dto.Id} not found");
        }

        // Check if asset tag changed and if it's unique
        if (asset.AssetTag != dto.AssetTag && await ExistsAsync(dto.AssetTag))
        {
            throw new DomainException($"Asset with tag {dto.AssetTag} already exists");
        }

        dto.Adapt(asset);
        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

        await _assetRepository.UpdateAsync(asset);
        await _assetRepository.SaveChangesAsync();

        _logger.LogInformation("Asset updated: {AssetTag} by {User}", asset.AssetTag, asset.UpdatedBy);

        return asset.Adapt<AssetDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null)
        {
            return false;
        }

        // Soft delete or hard delete based on requirements
        asset.Status = "Deleted";
        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";
        
        await _assetRepository.UpdateAsync(asset);
        await _assetRepository.SaveChangesAsync();

        _logger.LogInformation("Asset deleted: {AssetTag} by {User}", asset.AssetTag, asset.UpdatedBy);

        return true;
    }

    public async Task<bool> ExistsAsync(string assetTag)
    {
        return await _assetRepository.ExistsAsync(a => a.AssetTag == assetTag);
    }

    public async Task<int> GetCountAsync()
    {
        return await _assetRepository.GetCountAsync();
    }

    public async Task<List<AssetDto>> GetRecentAsync(int count)
    {
        var assets = await _assetRepository.GetRecentAsync(count);
        return assets.Adapt<List<AssetDto>>();
    }

    public async Task<byte[]> ExportToExcelAsync(AssetSearchDto search)
    {
        // Get all data for export (no paging)
        search.Page = 1;
        search.PageSize = int.MaxValue;
        var result = await GetPagedAsync(search);
        
        // Generate Excel file
        // You'll need to add EPPlus or ClosedXML NuGet package
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Assets");
        
        // Add headers
        worksheet.Cells[1, 1].Value = "Asset Tag";
        worksheet.Cells[1, 2].Value = "Make";
        worksheet.Cells[1, 3].Value = "Model";
        worksheet.Cells[1, 4].Value = "Year";
        worksheet.Cells[1, 5].Value = "Serial Number";
        worksheet.Cells[1, 6].Value = "VIN";
        worksheet.Cells[1, 7].Value = "Status";
        worksheet.Cells[1, 8].Value = "Compliance Status";
        worksheet.Cells[1, 9].Value = "Insured Value";
        worksheet.Cells[1, 10].Value = "Created At";
        
        // Add data
        var row = 2;
        foreach (var asset in result.Items)
        {
            worksheet.Cells[row, 1].Value = asset.AssetTag;
            worksheet.Cells[row, 2].Value = asset.Make;
            worksheet.Cells[row, 3].Value = asset.Model;
            worksheet.Cells[row, 4].Value = asset.Year;
            worksheet.Cells[row, 5].Value = asset.SerialNumber;
            worksheet.Cells[row, 6].Value = asset.VIN;
            worksheet.Cells[row, 7].Value = asset.Status;
            worksheet.Cells[row, 8].Value = asset.ComplianceStatus;
            worksheet.Cells[row, 9].Value = asset.InsuredValue;
            worksheet.Cells[row, 10].Value = asset.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }
        
        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }
}