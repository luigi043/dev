using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Exceptions;
using InsureX.Domain.Interfaces;

using Mapster;

namespace InsureX.Application.Services
{   
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
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;

        public AssetService(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<List<AssetDto>> GetAllAsync()
        {
            var assets = await _assetRepository.GetAllAsync();
            return assets.Adapt<List<AssetDto>>();
        }

        public async Task<AssetDto?> GetByIdAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            return asset?.Adapt<AssetDto>();
        }

        public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
        {
            var asset = dto.Adapt<Asset>();
            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();
            return asset.Adapt<AssetDto>();
        }

        public async Task<AssetDto?> UpdateAsync(UpdateAssetDto dto)
        {
            var asset = await _assetRepository.GetByIdAsync(dto.Id)
                        ?? throw new DomainException($"Asset {dto.Id} not found");

            dto.Adapt(asset);
            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();
            return asset.Adapt<AssetDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null) return false;

            await _assetRepository.DeleteAsync(asset);
            await _assetRepository.SaveChangesAsync();
            return true;
        }
    }
}