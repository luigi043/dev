using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;
using System;

namespace InsureX.Domain.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    // Query methods
    Task<IQueryable<Asset>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Asset> query);
    Task<List<Asset>> GetPagedAsync(IQueryable<Asset> query, int page, int pageSize);
    
    // Asset specific methods
    Task<Asset?> GetByAssetTagAsync(string assetTag);
    Task<int> GetCompliantCountAsync();
    Task<Dictionary<string, int>> GetCountByStatusAsync();
    
    // NEW METHODS - Add these to fix the errors
    Task<int> GetCountAsync(Func<Asset, bool> predicate);  // For line 202
    Task<List<Asset>> GetRecentAsync(int count, Guid tenantId);  // For line 212
    
    // Keep this one - it's specific to string parameter
    Task<bool> ExistsAsync(string assetTag);
}