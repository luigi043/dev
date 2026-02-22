using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces;

public interface IAssetService
{
    Task<PagedResult<AssetDto>> GetPagedAsync(AssetSearchDto search);
    Task<AssetDto?> GetByIdAsync(int id);
    Task<AssetDto> CreateAsync(CreateAssetDto dto);
    Task<AssetDto?> UpdateAsync(UpdateAssetDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string assetTag);
    Task<int> GetCountAsync();
    Task<List<AssetDto>> GetRecentAsync(int count);
    Task<byte[]> ExportToExcelAsync(AssetSearchDto search);
}