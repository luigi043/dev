using System.Linq.Expressions;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    // Query methods
    Task<IQueryable<Asset>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Asset> query);
    Task<List<Asset>> GetPagedAsync(IQueryable<Asset> query, int page, int pageSize);
    
    // Get by ID with includes
    new Task<Asset?> GetByIdAsync(int id);
    
    // Asset-specific queries
    Task<Asset?> GetByAssetTagAsync(string assetTag, Guid tenantId);
    Task<List<Asset>> GetByStatusAsync(string status, Guid tenantId);
    Task<List<Asset>> GetByComplianceStatusAsync(string complianceStatus, Guid tenantId);
    Task<List<Asset>> GetRecentAsync(Guid tenantId, int count);
    
    // Existence checks
    Task<bool> IsAssetTagUniqueAsync(string assetTag, Guid tenantId, int? excludeId = null);
    new Task<bool> ExistsAsync(Expression<Func<Asset, bool>> predicate);
    
    // Count methods
    Task<int> GetCountAsync(Guid tenantId);
    Task<int> GetCompliantCountAsync(Guid tenantId);
    Task<int> GetNonCompliantCountAsync(Guid tenantId);
    Task<Dictionary<string, int>> GetCountByStatusAsync(Guid tenantId);
}