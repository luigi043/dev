using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IPolicyRepository : IRepository<Policy>
{
    
    // Query methods
    Task<IQueryable<Policy>> GetQueryableAsync();
    Task<int> CountAsync(IQueryable<Policy> query);
    Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize);
    
    // Policy specific methods
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
    Task<List<Policy>> GetExpiringPoliciesAsync(int days);
    Task<List<Policy>> GetExpiredPoliciesAsync();
    Task<List<Policy>> GetByAssetIdAsync(int assetId);
    Task<int> GetActiveCountAsync();
    Task<int> GetExpiringCountAsync(int days);
    
    // Claims methods
    Task<PolicyClaim?> GetClaimByIdAsync(int claimId);
    Task<List<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
    Task AddClaimAsync(PolicyClaim claim);
    Task UpdateClaimAsync(PolicyClaim claim);
    
    // Keep this one - it's specific to string parameter
    Task<bool> ExistsAsync(string policyNumber);
    
    // REMOVE these - they're already in IRepository<T>!
    // Task<Policy?> GetByIdAsync(int id);         
    // Task AddAsync(Policy policy);                
    // Task UpdateAsync(Policy policy);             
    // Task DeleteAsync(int id);                    
    // Task SaveChangesAsync();                     
}