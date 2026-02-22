using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<List<PolicyDto>> GetAllAsync();
        Task<PolicyDto?> GetByIdAsync(int id);
        Task<PolicyDto> CreateAsync(CreatePolicyDto dto);
        Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days);
    }
}