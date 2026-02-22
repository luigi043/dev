using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    // Only add EXTENSION methods here - NOT the basic CRUD ones!
    
    // Query methods
    Task<IQueryable<Asset>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Asset> query);
    Task<List<Asset>> GetPagedAsync(IQueryable<Asset> query, int page, int pageSize);
    
    // Asset specific methods
    Task<Asset?> GetByAssetTagAsync(string assetTag);
    Task<int> GetCompliantCountAsync();
    Task<Dictionary<string, int>> GetCountByStatusAsync();
    
    // REMOVE these - they're already in IRepository<T>!
    // Task<Asset?> GetByIdAsync(int id);         
    // Task<int> GetCountAsync();                  
    // Task<List<Asset>> GetRecentAsync(int count); 
    // Task<bool> ExistsAsync(string assetTag);    <- KEEP (specific to string parameter)
    // Task AddAsync(Asset asset);                  
    // Task UpdateAsync(Asset asset);               
    // Task DeleteAsync(int id);                    
    // Task SaveChangesAsync();                     
    
    // Keep this one - it's specific to string parameter
    Task<bool> ExistsAsync(string assetTag);
}