using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ITenantContext = InsureX.Domain.Interfaces.ITenantContext;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Exceptions;
using InsureX.Domain.Interfaces;
using Mapster;
using OfficeOpenXml;
using InsureX.Application.Interfaces;

namespace InsureX.Application.Services
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ICurrentUserService _currentUserService;  // Keep only ONE declaration
        private readonly ILogger<AssetService> _logger;

        public AssetService(
            IAssetRepository assetRepository,
            ITenantContext tenantContext,
            ICurrentUserService currentUserService,  // Use ICurrentUserService directly
            ILogger<AssetService> logger)
        {
            _assetRepository = assetRepository;
            _tenantContext = tenantContext;
            _currentUserService = currentUserService;  // Remove any duplicate assignments
            _logger = logger;
        }

        public async Task<PagedResult<AssetDto>> GetPagedAsync(AssetSearchDto search)
        {
            try
            {
                var tenantId = _tenantContext.GetCurrentTenantId()
                               ?? throw new UnauthorizedAccessException("Tenant not found");

                var query = await _assetRepository.GetQueryableAsync();

                // Always filter by tenant and exclude deleted
                query = query.Where(a =>
                    a.TenantId == tenantId &&
                    a.Status != "Deleted");

                // Search filter (null safe)
                if (!string.IsNullOrWhiteSpace(search.SearchTerm))
                {
                    var term = search.SearchTerm.Trim();

                    query = query.Where(a =>
                        (a.AssetTag ?? "").Contains(term) ||
                        (a.Make ?? "").Contains(term) ||
                        (a.Model ?? "").Contains(term) ||
                        (a.SerialNumber ?? "").Contains(term) ||
                        (a.VIN ?? "").Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(search.Status))
                    query = query.Where(a => a.Status == search.Status);

                if (!string.IsNullOrWhiteSpace(search.ComplianceStatus))
                    query = query.Where(a => a.ComplianceStatus == search.ComplianceStatus);

                if (search.Year.HasValue)
                    query = query.Where(a => a.Year == search.Year.Value);

                if (search.FromDate.HasValue)
                    query = query.Where(a => a.CreatedAt >= search.FromDate.Value);

                if (search.ToDate.HasValue)
                    query = query.Where(a => a.CreatedAt <= search.ToDate.Value);

                // Sorting
                bool asc = search.SortDir?.ToLower() == "asc";

                query = search.SortBy?.ToLower() switch
                {
                    "assettag" => asc ? query.OrderBy(a => a.AssetTag)
                                      : query.OrderByDescending(a => a.AssetTag),
                    "make" => asc ? query.OrderBy(a => a.Make)
                                  : query.OrderByDescending(a => a.Make),
                    "model" => asc ? query.OrderBy(a => a.Model)
                                   : query.OrderByDescending(a => a.Model),
                    "year" => asc ? query.OrderBy(a => a.Year)
                                  : query.OrderByDescending(a => a.Year),
                    "status" => asc ? query.OrderBy(a => a.Status)
                                    : query.OrderByDescending(a => a.Status),
                    _ => asc ? query.OrderBy(a => a.CreatedAt)
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
            var tenantId = _tenantContext.GetCurrentTenantId()
                           ?? throw new UnauthorizedAccessException();

            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null || asset.TenantId != tenantId || asset.Status == "Deleted")
                return null;

            return asset.Adapt<AssetDto>();
        }

        public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
        {
            if (await ExistsAsync(dto.AssetTag))
                throw new DomainException($"Asset with tag {dto.AssetTag} already exists");

            var tenantId = _tenantContext.GetCurrentTenantId()
                           ?? throw new UnauthorizedAccessException();

            var asset = dto.Adapt<Asset>();
            asset.TenantId = tenantId;
            asset.CreatedBy = _currentUserService.GetCurrentUserId() ?? "system";
            asset.CreatedAt = DateTime.UtcNow;
            asset.Status = "Active";
            asset.ComplianceStatus = "Pending";

            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            return asset.Adapt<AssetDto>();
        }

        public async Task<AssetDto?> UpdateAsync(UpdateAssetDto dto)
        {
            var asset = await _assetRepository.GetByIdAsync(dto.Id)
                        ?? throw new DomainException($"Asset with id {dto.Id} not found");

            if (asset.Status == "Deleted")
                throw new DomainException("Cannot update deleted asset");

            if (asset.AssetTag != dto.AssetTag && await ExistsAsync(dto.AssetTag))
                throw new DomainException($"Asset with tag {dto.AssetTag} already exists");

            dto.Adapt(asset);
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();

            return asset.Adapt<AssetDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null || asset.Status == "Deleted")
                return false;

            asset.Status = "Deleted";
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(string assetTag)
        {
            var tenantId = _tenantContext.GetCurrentTenantId()
                           ?? throw new UnauthorizedAccessException();

            return await _assetRepository.ExistsAsync(a =>
                a.AssetTag == assetTag &&
                a.TenantId == tenantId &&
                a.Status != "Deleted");
        }

        public async Task<int> GetCountAsync()
        {
            var tenantId = _tenantContext.GetCurrentTenantId()
                        ?? throw new UnauthorizedAccessException();

            // Alternative: Use GetQueryableAsync + CountAsync
            var query = await _assetRepository.GetQueryableAsync();
            query = query.Where(a => a.TenantId == tenantId && a.Status != "Deleted");
            return await _assetRepository.CountAsync(query);
        }

        public async Task<List<AssetDto>> GetRecentAsync(int count)
        {
            var tenantId = _tenantContext.GetCurrentTenantId()
                        ?? throw new UnauthorizedAccessException();

            // Alternative: Use GetQueryableAsync + manual ordering
            var query = await _assetRepository.GetQueryableAsync();
            query = query.Where(a => a.TenantId == tenantId && a.Status != "Deleted")
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(count);
            
            var items = await _assetRepository.GetPagedAsync(query, 1, count);
            return items.Adapt<List<AssetDto>>();
        }

        public async Task<byte[]> ExportToExcelAsync(AssetSearchDto search)
        {
            // For EPPlus 8+
            ExcelPackage.License.SetNonCommercialPersonal("InsureX User");

            search.Page = 1;
            search.PageSize = int.MaxValue;

            var result = await GetPagedAsync(search);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Assets");

            var headers = new[]
            {
                "Asset Tag", "Make", "Model", "Year",
                "Serial Number", "VIN", "Status",
                "Compliance Status", "Insured Value", "Created At"
            };

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cells[1, i + 1].Value = headers[i];

            int row = 2;

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
}