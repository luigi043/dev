using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
namespace InsureX.Domain.Interfaces;

public interface IAssetRepository : IRepository<Asset>
{
    Task<int> GetCountAsync();
    Task<int> GetCompliantCountAsync();
    Task<Dictionary<string, int>> GetCountByStatusAsync();
}