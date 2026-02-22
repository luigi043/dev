using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces
{
    public interface IPolicyRepository
    {
        Task<IQueryable<Policy>> GetQueryableAsync();
        Task<bool> ExistsAsync(string policyNumber);
        Task AddAsync(Policy policy);
        Task UpdateAsync(Policy policy);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<List<Policy>> GetExpiringPoliciesAsync(int days);
        Task<List<Policy>> GetByAssetIdAsync(int assetId);
        Task<Policy?> GetByIdAsync(int id);
    }
}