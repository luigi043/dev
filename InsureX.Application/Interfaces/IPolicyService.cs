using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces;

public interface IPolicyService
{
    // Policy CRUD
    Task<PagedResult<PolicyDto>> GetPagedAsync(PolicySearchDto search);
    Task<PolicyDto?> GetByIdAsync(int id);
    Task<PolicyDto> CreateAsync(CreatePolicyDto dto);
    Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto);
    Task<bool> DeleteAsync(int id);
    
    // Policy queries
    Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days);
    Task<List<PolicyDto>> GetByAssetIdAsync(int assetId);
    Task<PolicySummaryDto> GetSummaryAsync();
    Task<bool> CheckComplianceAsync(int assetId);
    
    // Claims
    Task<ClaimDto?> GetClaimByIdAsync(int claimId);
    Task<List<ClaimDto>> GetClaimsByPolicyIdAsync(int policyId);
    Task<ClaimDto> AddClaimAsync(CreateClaimDto dto);
    Task<ClaimDto?> UpdateClaimStatusAsync(int claimId, string status, decimal? settlementAmount);
    
    // Batch operations
    Task<int> UpdateExpiredPoliciesAsync();
    Task<int> SendRenewalRemindersAsync(int daysBeforeExpiry);
}