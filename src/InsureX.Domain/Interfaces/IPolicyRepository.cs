using InsureX.Domain.Entities;
using System.Linq.Expressions;

namespace InsureX.Domain.Interfaces;

public interface IPolicyRepository : IRepository<Policy>
{
    Task<IQueryable<Policy>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Policy> query);
    Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize);
    Task<Policy?> GetByIdAsync(int id);
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
    Task<List<Policy>> GetExpiringPoliciesAsync(int days);
    Task<List<Policy>> GetExpiredPoliciesAsync();
    Task<List<Policy>> GetByAssetIdAsync(int assetId);
    Task<Dictionary<string, int>> GetPolicySummaryAsync();
    Task<bool> ExistsAsync(string policyNumber);
    Task<int> GetActiveCountAsync();
    Task<int> GetExpiringCountAsync(int days);
    
    // Claims
    Task<PolicyClaim?> GetClaimByIdAsync(int claimId);
    Task<List<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
    Task AddClaimAsync(PolicyClaim claim);
    Task UpdateClaimAsync(PolicyClaim claim);
}