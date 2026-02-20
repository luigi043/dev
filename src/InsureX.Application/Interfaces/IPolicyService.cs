using InsureX.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace InsureX.Application.Interfaces;

public interface IPolicyRepository
{
    Task<IQueryable<Policy>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Policy> query);
    Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize);
    Task<Policy?> GetByIdAsync(int id);
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
    Task<List<Policy>> GetExpiringPoliciesAsync(int days);
    Task<List<Policy>> GetByAssetIdAsync(int assetId);
    Task AddAsync(Policy policy);
    Task UpdateAsync(Policy policy);
    Task<bool> ExistsAsync(string policyNumber);
    Task<int> GetActiveCountAsync();
    Task<int> GetExpiringCountAsync(int days);
    Task SaveChangesAsync();
}