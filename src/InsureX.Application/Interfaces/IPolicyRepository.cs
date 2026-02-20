using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces;

public interface IPolicyService
{
    Task<PagedResult<PolicyDto>> GetPagedAsync(PolicySearchDto search);
    Task<PolicyDto?> GetByIdAsync(int id);
    Task<PolicyDto> CreateAsync(CreatePolicyDto dto);
    Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days);
    Task<ClaimDto> AddClaimAsync(CreateClaimDto dto);
    Task<List<ClaimDto>> GetClaimsByPolicyIdAsync(int policyId);
}