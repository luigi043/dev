using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces
{
    public interface IAssetService
    {
        // Queries
        Task<PagedResult<AssetDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
        Task<AssetDto?> GetByIdAsync(int id);
        Task<List<AssetDto>> GetByTenantIdAsync(int tenantId);
        Task<List<AssetDto>> GetAssetsByStatusAsync(string status);
        
        // Commands
        Task<AssetDto> CreateAsync(CreateAssetDto dto);
        Task<AssetDto> UpdateAsync(int id, UpdateAssetDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ArchiveAsync(int id);
        
        // Search
        Task<List<AssetDto>> SearchAsync(string searchTerm, int maxResults = 10);
        Task<PagedResult<AssetDto>> AdvancedSearchAsync(AssetSearchDto searchDto);
        
        // Batch Operations
        Task<int> BulkUpdateStatusAsync(List<int> assetIds, string status);
        Task<int> BulkArchiveAsync(List<int> assetIds);
        
        // Validation
        Task<bool> AssetTagExistsAsync(string assetTag, int? excludeId = null);
        Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeId = null);
        
        // Dashboard/Statistics
        Task<int> GetTotalCountAsync();
        Task<Dictionary<string, int>> GetStatusDistributionAsync();
    }
}