using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;
using System.Collections.Generic;

namespace InsureX.Domain.Interfaces
{
    public interface IAssetRepository
    {
        Task<IQueryable<Asset>> GetQueryableAsync();
        Task<Asset?> GetByIdAsync(int id);
        Task AddAsync(Asset asset);
        Task UpdateAsync(Asset asset);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<List<Asset>> GetPagedAsync(int page, int pageSize);
        Task<int> GetCountAsync();
        Task<List<Asset>> GetRecentAsync(int count);
    }
}