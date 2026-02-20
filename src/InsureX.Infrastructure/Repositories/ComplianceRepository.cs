using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;
using System.Text.Json;
using System.Linq.Expressions;

namespace InsureX.Infrastructure.Repositories;

public class ComplianceRepository : Repository<ComplianceCheck>, IComplianceRepository
{
    public ComplianceRepository(AppDbContext context) : base(context)
    {
    }

    // Compliance Rules
    public async Task<List<ComplianceRule>> GetActiveRulesAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Set<ComplianceRule>()
            .Where(r => r.IsActive && 
                       (r.EffectiveFrom == null || r.EffectiveFrom <= now) &&
                       (r.EffectiveTo == null || r.EffectiveTo >= now))
            .OrderBy(r => r.Priority)
            .ToListAsync();
    }

    public async Task<ComplianceRule?> GetRuleByCodeAsync(string ruleCode)
    {
        return await _context.Set<ComplianceRule>()
            .FirstOrDefaultAsync(r => r.RuleCode == ruleCode);
    }

    public async Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType)
    {
        return await _context.Set<ComplianceRule>()
            .Where(r => r.RuleType == ruleType && r.IsActive)
            .ToListAsync();
    }

    public async Task<ComplianceRule?> GetRuleByIdAsync(int id)
    {
        return await _context.Set<ComplianceRule>()
            .FindAsync(id);
    }

    public async Task AddRuleAsync(ComplianceRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        await _context.Set<ComplianceRule>().AddAsync(rule);
    }

    public async Task UpdateRuleAsync(ComplianceRule rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.Set<ComplianceRule>().Update(rule);
        await Task.CompletedTask;
    }

    // Compliance Checks
    public async Task<ComplianceCheck?> GetLatestCheckAsync(int assetId)
    {
        return await _context.ComplianceChecks
            .Include(c => c.Rule)
            .Where(c => c.AssetId == assetId)
            .OrderByDescending(c => c.CheckDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.ComplianceChecks
            .Include(c => c.Rule)
            .Where(c => c.AssetId == assetId && c.CheckDate >= cutoff)
            .OrderByDescending(c => c.CheckDate)
            .ToListAsync();
    }

    public async Task<List<ComplianceCheck>> GetAssetsNeedingCheckAsync(int hoursThreshold = 24)
    {
        var threshold = DateTime.UtcNow.AddHours(-hoursThreshold);
        return await _context.ComplianceChecks
            .Include(c => c.Asset)
            .Where(c => c.CheckDate <= threshold || c.NextCheckDate <= DateTime.UtcNow)
            .ToListAsync();
    }

    // Compliance History
    public async Task<List<ComplianceHistory>> GetHistoryAsync(int assetId, int days = 90)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.Set<ComplianceHistory>()
            .Where(h => h.AssetId == assetId && h.ChangeDate >= cutoff)
            .OrderByDescending(h => h.ChangeDate)
            .ToListAsync();
    }

    public async Task AddHistoryAsync(ComplianceHistory history)
    {
        history.CreatedAt = DateTime.UtcNow;
        await _context.Set<ComplianceHistory>().AddAsync(history);
    }

    // Alerts
    public async Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null)
    {
        var query = _context.ComplianceAlerts
            .Include(a => a.Asset)
            .Where(a => a.Status == "New" || a.Status == "Acknowledged");

        if (assetId.HasValue)
        {
            query = query.Where(a => a.AssetId == assetId.Value);
        }

        return await query
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ComplianceAlert>> GetAlertsByAssetAsync(int assetId)
    {
        return await _context.ComplianceAlerts
            .Where(a => a.AssetId == assetId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<ComplianceAlert?> GetAlertByIdAsync(int id)
    {
        return await _context.ComplianceAlerts
            .Include(a => a.Asset)
            .Include(a => a.Rule)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAlertAsync(ComplianceAlert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        await _context.ComplianceAlerts.AddAsync(alert);
    }

    public async Task UpdateAlertAsync(ComplianceAlert alert)
    {
        alert.UpdatedAt = DateTime.UtcNow;
        _context.ComplianceAlerts.Update(alert);
        await Task.CompletedTask;
    }

    public async Task<int> GetActiveAlertCountAsync(int? assetId = null)
    {
        var query = _context.ComplianceAlerts
            .Where(a => a.Status == "New" || a.Status == "Acknowledged");

        if (assetId.HasValue)
        {
            query = query.Where(a => a.AssetId == assetId.Value);
        }

        return await query.CountAsync();
    }

    // Dashboard
    public async Task<ComplianceDashboard?> GetLatestDashboardAsync(Guid tenantId)
    {
        return await _context.Set<ComplianceDashboard>()
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.SnapshotDate)
            .FirstOrDefaultAsync();
    }

    public async Task SaveDashboardAsync(ComplianceDashboard dashboard)
    {
        dashboard.SnapshotDate = DateTime.UtcNow;
        await _context.Set<ComplianceDashboard>().AddAsync(dashboard);
    }

    public async Task<Dictionary<string, int>> GetComplianceSummaryAsync()
    {
        return await _context.Assets
            .GroupBy(a => a.ComplianceStatus ?? "Unknown")
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);
    }

    // Statistics
    public async Task<double> GetAverageComplianceScoreAsync()
    {
        var latestChecks = await _context.ComplianceChecks
            .GroupBy(c => c.AssetId)
            .Select(g => g.OrderByDescending(c => c.CheckDate).FirstOrDefault())
            .ToListAsync();

        return latestChecks.Any() ? latestChecks.Average(c => c?.Score ?? 0) : 0;
    }

    public async Task<int> GetAssetsByStatusCountAsync(string status)
    {
        return await _context.Assets
            .CountAsync(a => a.ComplianceStatus == status);
    }

    public async Task<List<Asset>> GetNonCompliantAssetsAsync(int severity = 0)
    {
        var query = _context.Assets
            .Include(a => a.Policies)
            .Where(a => a.ComplianceStatus == "Non-Compliant");

        if (severity > 0)
        {
            // Filter by severity of active alerts
            query = query.Where(a => _context.ComplianceAlerts
                .Any(al => al.AssetId == a.Id && 
                          al.Severity >= severity && 
                          al.Status != "Resolved"));
        }

        return await query.ToListAsync();
    }
}