using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;
using InsureX.Application.DTOs;
using System.Linq.Expressions;

namespace InsureX.Infrastructure.Repositories
{
    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(AppDbContext context) : base(context)
        {
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

        public new async Task<Policy?> GetByIdAsync(int id)
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
                .Where(p => p.EndDate <= expiryDate && 
                           p.EndDate >= DateTime.UtcNow &&
                           p.Status == "Active")
                .ToListAsync();
        }

        public async Task<List<Policy>> GetExpiredPoliciesAsync()
        {
            return await _context.Policies
                .Where(p => p.EndDate < DateTime.UtcNow && p.Status == "Active")
                .ToListAsync();
        }

        public async Task<List<Policy>> GetByAssetIdAsync(int assetId)
        {
            return await _context.Policies
                .Where(p => p.AssetId == assetId)
                .ToListAsync();
        }

        public async Task<PolicyStatistics> GetSummaryAsync()
        {
            var policies = await _context.Policies.ToListAsync();
            var now = DateTime.UtcNow;

            return new PolicyStatistics
            {
                TotalPolicies = policies.Count,
                ActivePolicies = policies.Count(p => p.Status == "Active" && p.EndDate >= now),
                ExpiringPolicies = policies.Count(p => p.Status == "Active" && 
                    p.EndDate >= now && p.EndDate <= now.AddDays(30)),
                ExpiredPolicies = policies.Count(p => p.EndDate < now),
                TotalSumInsured = policies.Sum(p => p.SumInsured),
                TotalPremium = policies.Sum(p => p.Premium),
                PoliciesByType = policies
                    .GroupBy(p => p.PolicyType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                PoliciesByInsurer = policies
                    .GroupBy(p => p.InsurerName)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
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

        // Claims methods
        public async Task<PolicyClaim?> GetClaimByIdAsync(int claimId)
        {
            return await _context.Set<PolicyClaim>()
                .FirstOrDefaultAsync(c => c.Id == claimId);
        }

        public async Task<List<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId)
        {
            return await _context.Set<PolicyClaim>()
                .Where(c => c.PolicyId == policyId)
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
}
