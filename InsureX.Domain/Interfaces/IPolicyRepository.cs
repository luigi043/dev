using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IPolicyRepository : IRepository<Policy>
{
    Task<Policy?> GetPolicyWithDetailsAsync(int id);
    Task<IEnumerable<Policy>> GetByAssetIdAsync(int assetId);
    Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(int days);
    Task<IEnumerable<Policy>> GetExpiredPoliciesAsync();
    Task<bool> ExistsAsync(string policyNumber);
    Task<IQueryable<Policy>> GetQueryableAsync();
    Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize);
    Task<int> CountAsync(IQueryable<Policy> query);
    
    // Claims
    Task<PolicyClaim?> GetClaimByIdAsync(int claimId);
    Task<IEnumerable<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
    Task AddClaimAsync(PolicyClaim claim);
    Task UpdateClaimAsync(PolicyClaim claim);
}