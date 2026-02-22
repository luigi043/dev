using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Exceptions;
using InsureX.Domain.Interfaces;
using Mapster;
using OfficeOpenXml; 



namespace InsureX.Application.Services
{
    public class DashboardService : IDashboardService
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
            if (await ExistsAsync(dto.AssetTag))
                throw new DomainException($"Asset with tag {dto.AssetTag} already exists");

            var asset = dto.Adapt<Asset>();
            asset.TenantId = _tenantContext.GetCurrentTenantId() ?? throw new UnauthorizedAccessException();
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
            var asset = await _assetRepository.GetByIdAsync(dto.Id)
                        ?? throw new DomainException($"Asset with id {dto.Id} not found");

            if (asset.AssetTag != dto.AssetTag && await ExistsAsync(dto.AssetTag))
                throw new DomainException($"Asset with tag {dto.AssetTag} already exists");

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
            if (asset == null) return false;

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

        public Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var summary = new DashboardSummaryDto
            {
                TotalPolicies = 0,
                TotalAssets = 0
            };

            return Task.FromResult(summary);
        }
        public async Task<byte[]> ExportToExcelAsync(AssetSearchDto search)
        {
            search.Page = 1;
            search.PageSize = int.MaxValue;
            var result = await GetPagedAsync(search);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Assets");

            // Headers
            var headers = new[]
            {
                "Asset Tag", "Make", "Model", "Year", "Serial Number",
                "VIN", "Status", "Compliance Status", "Insured Value", "Created At"
            };
            for (int i = 0; i < headers.Length; i++)
                worksheet.Cells[1, i + 1].Value = headers[i];

            // Data
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