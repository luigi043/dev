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
        return await Task.FromResult(_context.Assets
            .Include(a => a.Policies)
            .AsQueryable());
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

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.Policies)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Assets.CountAsync();
    }

    public async Task<int> GetCompliantCountAsync()
    {
        return await _context.Assets
            .CountAsync(a => a.ComplianceStatus == "Compliant");
    }

    public async Task<Dictionary<string, int>> GetCountByStatusAsync()
    {
        return await _context.Assets
            .GroupBy(a => a.Status)
            .Where(g => g.Key != "Deleted")
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);
    }

    public async Task<List<Asset>> GetRecentAsync(int count)
    {
        return await _context.Assets
            .Where(a => a.Status != "Deleted")
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<Asset, bool>> predicate)
    {
        return await _context.Assets.AnyAsync(predicate);
    }
}