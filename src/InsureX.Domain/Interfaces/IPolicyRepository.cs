using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Domain.Entities;

namespace InsureX.Application.Interfaces
{
    public interface IPolicyRepository
    {
        Task<Policy?> GetByIdAsync(int id);
        Task<List<Policy>> GetAllAsync();
        Task AddAsync(Policy policy);
        Task UpdateAsync(Policy policy);
        Task DeleteAsync(int id);
    }
}