using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Application.Interfaces;  // ADD THIS
using InsureX.Infrastructure.Data;
using System;  // ADD THIS
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InsureX.Infrastructure.Repositories;

public class ComplianceRepository : IComplianceRepository
{
    private readonly AppDbContext _context;

    public ComplianceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComplianceRule>> GetActiveRulesAsync()
    {
        return await _context.Set<ComplianceRule>()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToListAsync();
    }

    public async Task<ComplianceRule?> GetRuleByIdAsync(int id)
    {
        return await _context.Set<ComplianceRule>().FindAsync(id);
    }

    public async Task AddRuleAsync(ComplianceRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        await _context.Set<ComplianceRule>().AddAsync(rule);
    }

    public Task UpdateRuleAsync(ComplianceRule rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.Set<ComplianceRule>().Update(rule);
        return Task.CompletedTask;
    }

    public async Task<ComplianceCheck?> GetLatestCheckAsync(int assetId)
    {
        return await _context.ComplianceChecks
            .Where(c => c.AssetId == assetId)
            .OrderByDescending(c => c.CheckDate)
            .FirstOrDefaultAsync();
    }

    public async Task AddCheckAsync(ComplianceCheck check)
    {
        check.CreatedAt = DateTime.UtcNow;
        await _context.ComplianceChecks.AddAsync(check);
    }

    public async Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null)
    {
        var query = _context.ComplianceAlerts
            .Include(a => a.Asset)
            .Where(a => a.Status == "New" || a.Status == "Acknowledged");

        if (assetId.HasValue)
        {
            query = query.Where(a => a.AssetId == assetId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<ComplianceAlert?> GetAlertByIdAsync(int id)
    {
        return await _context.ComplianceAlerts
            .Include(a => a.Asset)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAlertAsync(ComplianceAlert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        await _context.ComplianceAlerts.AddAsync(alert);
    }

    public Task UpdateAlertAsync(ComplianceAlert alert)
    {
        alert.UpdatedAt = DateTime.UtcNow;
        _context.ComplianceAlerts.Update(alert);
        return Task.CompletedTask;
    }

    public async Task<int> GetActiveAlertCountAsync()
    {
        return await _context.ComplianceAlerts
            .CountAsync(a => a.Status == "New" || a.Status == "Acknowledged");
    }

    public async Task AddHistoryAsync(ComplianceHistory history)
    {
        history.CreatedAt = DateTime.UtcNow;
        await _context.Set<ComplianceHistory>().AddAsync(history);
    }

    public async Task<Dictionary<string, int>> GetComplianceSummaryAsync()
    {
        return await _context.Assets
            .GroupBy(a => a.ComplianceStatus ?? "Unknown")
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}