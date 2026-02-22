using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using InsureX.Application.DTOs;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Entities;
using InsureX.Domain.Exceptions;
using InsureX.Domain.Interfaces;
using Mapster;

namespace InsureX.Application.Services
{
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
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<AssetDto>> GetPagedAsync(AssetSearchDto search)
        {
            try
            {
                // FIXED: Use TenantId property instead of GetCurrentTenantId() method
                var tenantId = _tenantContext.HasTenant 
                    ? _tenantContext.TenantId 
                    : throw new UnauthorizedAccessException("Tenant context not found");

                _logger.LogInformation("Getting paged assets for tenant {TenantId}, page {Page}", tenantId, search.Page);

                var query = await _assetRepository.GetQueryableAsync();

                // Apply tenant filter and exclude deleted
                query = query.Where(a =>
                    a.TenantId == tenantId &&
                    a.Status != "Deleted");

                // Apply search filters
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

                // Apply sorting
                query = ApplySorting(query, search.SortBy, search.SortDir);

                // Get total count
                var totalItems = await _assetRepository.CountAsync(query);

                // Get paged results
                var items = await _assetRepository.GetPagedAsync(query, search.Page, search.PageSize);

                return new PagedResult<AssetDto>
                {
                    Items = items.Adapt<List<AssetDto>>(),
                    Page = search.Page,
                    PageSize = search.PageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)search.PageSize),
                    HasNext = search.Page * search.PageSize < totalItems
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
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            var asset = await _assetRepository.GetByIdAsync(id);

            if (asset == null || asset.TenantId != tenantId || asset.Status == "Deleted")
                return null;

            return asset.Adapt<AssetDto>();
        }

        public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            // Check if asset tag already exists
            var isUnique = await _assetRepository.IsAssetTagUniqueAsync(dto.AssetTag, tenantId);
            if (!isUnique)
                throw new DomainException($"Asset with tag '{dto.AssetTag}' already exists");

            var asset = dto.Adapt<Asset>();
            asset.TenantId = tenantId;
            // FIXED: Use UserId property instead of GetCurrentUserId() method
            asset.CreatedBy = _currentUserService.UserId ?? "system";
            asset.CreatedAt = DateTime.UtcNow;
            asset.Status = "Active";
            asset.ComplianceStatus = "Pending";

            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Created asset {AssetTag} for tenant {TenantId}", asset.AssetTag, tenantId);

            return asset.Adapt<AssetDto>();
        }

        public async Task<AssetDto?> UpdateAsync(UpdateAssetDto dto)
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            var asset = await _assetRepository.GetByIdAsync(dto.Id)
                ?? throw new DomainException($"Asset with id {dto.Id} not found");

            if (asset.TenantId != tenantId)
                throw new UnauthorizedAccessException("Access denied to this asset");

            if (asset.Status == "Deleted")
                throw new DomainException("Cannot update deleted asset");

            // Check if asset tag is being changed and if it's unique
            if (asset.AssetTag != dto.AssetTag)
            {
                var isUnique = await _assetRepository.IsAssetTagUniqueAsync(dto.AssetTag, tenantId, dto.Id);
                if (!isUnique)
                    throw new DomainException($"Asset with tag '{dto.AssetTag}' already exists");
            }

            // Map DTO to existing entity
            dto.Adapt(asset);
            asset.UpdatedAt = DateTime.UtcNow;
            // FIXED: Use UserId property
            asset.UpdatedBy = _currentUserService.UserId ?? "system";

            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Updated asset {AssetId} for tenant {TenantId}", asset.Id, tenantId);

            return asset.Adapt<AssetDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            var asset = await _assetRepository.GetByIdAsync(id);
            
            if (asset == null || asset.TenantId != tenantId || asset.Status == "Deleted")
                return false;

            // Soft delete
            asset.Status = "Deleted";
            asset.UpdatedAt = DateTime.UtcNow;
            // FIXED: Use UserId property
            asset.UpdatedBy = _currentUserService.UserId ?? "system";

            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();

            _logger.LogInformation("Deleted asset {AssetId} for tenant {TenantId}", asset.Id, tenantId);

            return true;
        }

        public async Task<bool> ExistsAsync(string assetTag)
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            return await _assetRepository.ExistsAsync(a =>
                a.AssetTag == assetTag &&
                a.TenantId == tenantId &&
                a.Status != "Deleted");
        }

        public async Task<int> GetCountAsync()
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            var query = await _assetRepository.GetQueryableAsync();
            query = query.Where(a => a.TenantId == tenantId && a.Status != "Deleted");
            
            return await _assetRepository.CountAsync(query);
        }

        public async Task<List<AssetDto>> GetRecentAsync(int count)
        {
            // FIXED: Use TenantId property
            var tenantId = _tenantContext.HasTenant 
                ? _tenantContext.TenantId 
                : throw new UnauthorizedAccessException("Tenant context not found");

            var items = await _assetRepository.GetRecentAsync(tenantId, count);
            
            return items.Adapt<List<AssetDto>>();
        }

        public async Task<byte[]> ExportToExcelAsync(AssetSearchDto search)
        {
            // Set EPPlus license context
            ExcelPackage.License.SetNonCommercialPersonal("InsureX User");

            // Get data without pagination
            search.Page = 1;
            search.PageSize = int.MaxValue;

            var result = await GetPagedAsync(search);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Assets");

            // Headers
            var headers = new[]
            {
                "Asset Tag", "Make", "Model", "Year",
                "Serial Number", "VIN", "Status",
                "Compliance Status", "Value", "Insured Value",
                "Purchase Date", "Created At"
            };

            for (int i = 0; i < headers.Length; i++)
                worksheet.Cells[1, i + 1].Value = headers[i];

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

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
                worksheet.Cells[row, 9].Value = asset.Value;
                worksheet.Cells[row, 10].Value = asset.InsuredValue;
                worksheet.Cells[row, 11].Value = asset.PurchaseDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 12].Value = asset.CreatedAt.ToString("yyyy-MM-dd");
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return await Task.FromResult(package.GetAsByteArray());
        }

        #region Private Methods

        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string? sortBy, string? sortDir)
        {
            var isAscending = sortDir?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "assettag" => isAscending ? query.OrderBy(a => a.AssetTag)
                                          : query.OrderByDescending(a => a.AssetTag),
                "make" => isAscending ? query.OrderBy(a => a.Make)
                                      : query.OrderByDescending(a => a.Make),
                "model" => isAscending ? query.OrderBy(a => a.Model)
                                       : query.OrderByDescending(a => a.Model),
                "year" => isAscending ? query.OrderBy(a => a.Year)
                                      : query.OrderByDescending(a => a.Year),
                "status" => isAscending ? query.OrderBy(a => a.Status)
                                        : query.OrderByDescending(a => a.Status),
                "compliancestatus" => isAscending ? query.OrderBy(a => a.ComplianceStatus)
                                                  : query.OrderByDescending(a => a.ComplianceStatus),
                "value" => isAscending ? query.OrderBy(a => a.Value)
                                       : query.OrderByDescending(a => a.Value),
                "purchasedate" => isAscending ? query.OrderBy(a => a.PurchaseDate)
                                              : query.OrderByDescending(a => a.PurchaseDate),
                _ => isAscending ? query.OrderBy(a => a.CreatedAt)
                                 : query.OrderByDescending(a => a.CreatedAt)
            };
        }

        #endregion
    }
}