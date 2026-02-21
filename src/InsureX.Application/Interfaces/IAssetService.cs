// src/InsureX.Domain/Interfaces/IAssetRepository.cs
using System.Threading.Tasks;
using System.Collections.Generic;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces
{
    public interface IAssetRepository
    {
        Task<List<Asset>> GetAllAsync();
    }
}