using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;
using System.Linq.Expressions;

namespace InsureX.Infrastructure.Repositories;

public class AssetRepository : Repository<Asset>, IAssetRepository
{
    public AssetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IQueryable<Asset>> GetQueryableAsync()
    {
        return _context.Assets
            .Include(a => a.Policies)
            .Include(a => a.ComplianceResults)
            .AsQueryable();
    }

    public async Task<int> CountAsync(IQueryable<Asset> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<Asset>> GetPagedAsync(IQueryable<Asset> query, int page, int pageSize)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public new async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.Policies)
            .Include(a => a.ComplianceResults)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Asset?> GetByAssetTagAsync(string assetTag, Guid tenantId)
    {
        return await _context.Assets
            .FirstOrDefaultAsync(a => a.AssetTag == assetTag && a.TenantId == tenantId);
    }

    public async Task<List<Asset>> GetByStatusAsync(string status, Guid tenantId)
    {
        return await _context.Assets
            .Where(a => a.Status == status && a.TenantId == tenantId && a.Status != AssetStatusValues.Deleted)
            .ToListAsync();
    }

    public async Task<List<Asset>> GetByComplianceStatusAsync(string complianceStatus, Guid tenantId)
    {
        return await _context.Assets
            .Where(a => a.ComplianceStatus == complianceStatus && a.TenantId == tenantId && a.Status != AssetStatusValues.Deleted)
            .ToListAsync();
    }

    public async Task<List<Asset>> GetRecentAsync(Guid tenantId, int count)
    {
        return await _context.Assets
            .Where(a => a.TenantId == tenantId && a.Status != AssetStatusValues.Deleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> IsAssetTagUniqueAsync(string assetTag, Guid tenantId, int? excludeId = null)
    {
        var query = _context.Assets.Where(a => a.AssetTag == assetTag && a.TenantId == tenantId);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public new async Task<bool> ExistsAsync(Expression<Func<Asset, bool>> predicate)
    {
        return await _context.Assets.AnyAsync(predicate);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Assets.CountAsync();
    }

    public async Task<int> GetCompliantCountAsync()
    {
        return await _context.Assets
            .CountAsync(a => a.ComplianceStatus == ComplianceStatusValues.Compliant && 
                            a.Status != AssetStatusValues.Deleted);
    }

    public async Task<Dictionary<string, int>> GetCountByStatusAsync()
    {
        return await _context.Assets
            .Where(a => a.Status != AssetStatusValues.Deleted)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);
    }
}