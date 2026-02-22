using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;

namespace InsureX.Infrastructure.Repositories;

public class PolicyRepository : Repository<Policy>, IPolicyRepository
{
    public PolicyRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Policy?> GetPolicyWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Asset)
            .Include(p => p.Claims)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Policy>> GetByAssetIdAsync(int assetId)
    {
        return await _dbSet
            .Where(p => p.AssetId == assetId)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(int days)
    {
        var targetDate = DateTime.UtcNow.AddDays(days);
        return await _dbSet
            .Include(p => p.Asset)
            .Where(p => p.Status == "Active" 
                && p.EndDate <= targetDate 
                && p.EndDate >= DateTime.UtcNow)
            .OrderBy(p => p.EndDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Policy>> GetExpiredPoliciesAsync()
    {
        return await _dbSet
            .Where(p => p.EndDate < DateTime.UtcNow 
                && p.Status != "Expired" 
                && p.Status != "Deleted")
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string policyNumber)
    {
        return await _dbSet.AnyAsync(p => p.PolicyNumber == policyNumber);
    }

    public async Task<IQueryable<Policy>> GetQueryableAsync()
    {
        return await Task.FromResult(_dbSet.AsQueryable());
    }

    public async Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(IQueryable<Policy> query)
    {
        return await query.CountAsync();
    }

    // Claims
    public async Task<PolicyClaim?> GetClaimByIdAsync(int claimId)
    {
        return await _context.Set<PolicyClaim>()
            .Include(c => c.Policy)
            .FirstOrDefaultAsync(c => c.Id == claimId);
    }

    public async Task<IEnumerable<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId)
    {
        return await _context.Set<PolicyClaim>()
            .Where(c => c.PolicyId == policyId)
            .OrderByDescending(c => c.ClaimDate)
            .ToListAsync();
    }

    public async Task AddClaimAsync(PolicyClaim claim)
    {
        await _context.Set<PolicyClaim>().AddAsync(claim);
    }

    public async Task UpdateClaimAsync(PolicyClaim claim)
    {
        _context.Set<PolicyClaim>().Update(claim);
        await Task.CompletedTask;
    }
}