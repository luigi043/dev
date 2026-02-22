using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<List<PolicyDto>> GetAllAsync();
        Task<PolicyDto?> GetByIdAsync(Guid id);
        Task<PolicyDto> CreateAsync(CreatePolicyDto dto);
        Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days);
        Task<PolicyService.PolicySummaryDto> GetSummaryAsync();
    }
}