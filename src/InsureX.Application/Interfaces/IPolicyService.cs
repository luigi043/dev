using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Application.Interfaces
{
    public interface IPolicyService
    {
        Task<List<Policy>> GetAllAsync();
        Task<Policy?> GetByIdAsync(int id);
        Task<List<Policy>> GetExpiringPoliciesAsync(int days);
        Task AddAsync(Policy policy);
        Task UpdateAsync(Policy policy);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(string policyNumber);
        Task<int> GetActiveCountAsync();
        Task<int> GetExpiringCountAsync(int days);
        Task SaveChangesAsync();
    }
}