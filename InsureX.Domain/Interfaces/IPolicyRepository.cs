using System.Linq.Expressions;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IPolicyRepository : IRepository<Policy>
{
    // Policy specific methods with int IDs
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, int tenantId);
    Task<List<Policy>> GetExpiringPoliciesAsync(int tenantId, int days);
    Task<List<Policy>> GetExpiredPoliciesAsync(int tenantId);
    Task<List<Policy>> GetByAssetIdAsync(int assetId);
    Task<List<Policy>> GetByTenantIdAsync(int tenantId);
    Task<int> GetActiveCountAsync(int tenantId);
    Task<int> GetExpiringCountAsync(int tenantId, int days);
    
    // Claims methods with int IDs
    Task<PolicyClaim?> GetClaimByIdAsync(int claimId);
    Task<List<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
    Task<PolicyClaim> AddClaimAsync(PolicyClaim claim);
    Task UpdateClaimAsync(PolicyClaim claim);
    Task DeleteClaimAsync(int claimId);
    
    // Existence checks
    Task<bool> ExistsAsync(string policyNumber, int tenantId);
    Task<bool> PolicyNumberExistsAsync(string policyNumber, int tenantId, int? excludeId = null);
    
    // Advanced queries
    Task<List<Policy>> GetPoliciesByStatusAsync(string status, int tenantId);
    Task<List<Policy>> GetPoliciesByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetPolicyStatisticsAsync(int tenantId);
    Task<List<Policy>> GetPoliciesWithExpiringCoverageAsync(int tenantId, int daysThreshold = 30);
    
    // Batch operations
    Task<int> BulkRenewPoliciesAsync(List<int> policyIds, int tenantId);
    Task<int> BulkCancelPoliciesAsync(List<int> policyIds, string reason, int tenantId);
}