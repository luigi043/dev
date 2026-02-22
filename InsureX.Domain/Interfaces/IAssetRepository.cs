using System.Linq.Expressions;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    // Asset-specific queries with int tenantId
    Task<Asset?> GetByAssetTagAsync(string assetTag, int tenantId);
    Task<List<Asset>> GetByStatusAsync(string status, int tenantId);
    Task<List<Asset>> GetByComplianceStatusAsync(string complianceStatus, int tenantId);
    Task<List<Asset>> GetRecentAsync(int tenantId, int count);
    Task<List<Asset>> GetByTenantIdAsync(int tenantId);
    
    // Existence checks
    Task<bool> IsAssetTagUniqueAsync(string assetTag, int tenantId, int? excludeId = null);
    
    // Count methods with int tenantId
    Task<int> GetCountAsync(int tenantId);
    Task<int> GetCompliantCountAsync(int tenantId);
    Task<int> GetNonCompliantCountAsync(int tenantId);
    Task<Dictionary<string, int>> GetCountByStatusAsync(int tenantId);
    
    // Advanced queries
    Task<IEnumerable<Asset>> GetExpiringAssetsAsync(int tenantId, int daysThreshold = 30);
    Task<IEnumerable<Asset>> GetAssetsWithPendingInspectionsAsync(int tenantId);
    
    // Batch operations
    Task<int> BulkUpdateStatusAsync(List<int> assetIds, string status, int tenantId);
    Task<int> BulkArchiveAsync(List<int> assetIds, int tenantId);
}