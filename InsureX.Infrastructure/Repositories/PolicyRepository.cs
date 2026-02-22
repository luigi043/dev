using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Application.Interfaces;  // ADD THIS
using InsureX.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using InsureX.Domain.Interfaces;

namespace InsureX.Infrastructure.Repositories
{
    public class PolicyRepository : IPolicyRepository {

    private readonly AppDbContext _context;

    public PolicyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IQueryable<Policy>> GetQueryableAsync()
    {
        return await Task.FromResult(_context.Policies
            .Include(p => p.Asset)
            .AsQueryable());
    }

    public async Task<int> CountAsync(IQueryable<Policy> query)
    {
        return await query.CountAsync();
    }

    public async Task<List<Policy>> GetPagedAsync(IQueryable<Policy> query, int page, int pageSize)
    {
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Policy?> GetByIdAsync(int id)
    {
        return await _context.Policies
            .Include(p => p.Asset)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
    {
        return await _context.Policies
            .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
    }

    public async Task<List<Policy>> GetExpiringPoliciesAsync(int days)
    {
        var expiryDate = DateTime.UtcNow.AddDays(days);
        return await _context.Policies
            .Include(p => p.Asset)
            .Where(p => p.EndDate <= expiryDate && 
                       p.EndDate >= DateTime.UtcNow &&
                       p.Status == "Active")
            .ToListAsync();
    }

    public async Task<List<Policy>> GetByAssetIdAsync(int assetId)
    {
        return await _context.Policies
            .Where(p => p.AssetId == assetId)
            .ToListAsync();
    }

    public async Task AddAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
    }

    public Task UpdateAsync(Policy policy)
    {
        _context.Policies.Update(policy);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string policyNumber)
    {
        return await _context.Policies.AnyAsync(p => p.PolicyNumber == policyNumber);
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Policies
            .CountAsync(p => p.Status == "Active" && p.EndDate >= DateTime.UtcNow);
    }

    public async Task<int> GetExpiringCountAsync(int days)
    {
        var expiryDate = DateTime.UtcNow.AddDays(days);
        return await _context.Policies
            .CountAsync(p => p.Status == "Active" && 
                            p.EndDate <= expiryDate && 
                            p.EndDate >= DateTime.UtcNow);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}}