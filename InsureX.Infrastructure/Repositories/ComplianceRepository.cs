using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;
using System.Linq.Expressions;

namespace InsureX.Infrastructure.Repositories
{
    public class ComplianceRepository : Repository<ComplianceCheck>, IComplianceRepository
    {
        public ComplianceRepository(AppDbContext context) : base(context)
        {
        }

        // Implement all interface methods here
        public async Task<List<ComplianceRule>> GetActiveRulesAsync()
        {
            return await _context.Set<ComplianceRule>()
                .Where(r => r.IsActive)
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
            return await _context.Set<ComplianceRule>().FindAsync(id);
        }

        public async Task AddRuleAsync(ComplianceRule rule)
        {
            await _context.Set<ComplianceRule>().AddAsync(rule);
        }

        public async Task UpdateRuleAsync(ComplianceRule rule)
        {
            _context.Set<ComplianceRule>().Update(rule);
            await Task.CompletedTask;
        }

        public async Task<ComplianceCheck?> GetLatestCheckAsync(int assetId)
        {
            return await _context.ComplianceChecks
                .Where(c => c.AssetId == assetId)
                .OrderByDescending(c => c.CheckDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await _context.ComplianceChecks
                .Where(c => c.AssetId == assetId && c.CheckDate >= cutoff)
                .OrderByDescending(c => c.CheckDate)
                .ToListAsync();
        }

        public async Task<List<ComplianceCheck>> GetAssetsNeedingCheckAsync(int hoursThreshold = 24)
        {
            var threshold = DateTime.UtcNow.AddHours(-hoursThreshold);
            return await _context.ComplianceChecks
                .Where(c => c.CheckDate <= threshold)
                .ToListAsync();
        }

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
            await _context.Set<ComplianceHistory>().AddAsync(history);
        }

        public async Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null)
        {
            var query = _context.ComplianceAlerts
                .Where(a => a.Status == "New" || a.Status == "Acknowledged");

            if (assetId.HasValue)
            {
                query = query.Where(a => a.AssetId == assetId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ComplianceAlert>> GetAlertsByAssetAsync(int assetId)
        {
            return await _context.ComplianceAlerts
                .Where(a => a.AssetId == assetId)
                .ToListAsync();
        }

        public async Task<ComplianceAlert?> GetAlertByIdAsync(int id)
        {
            return await _context.ComplianceAlerts.FindAsync(id);
        }

        public async Task AddAlertAsync(ComplianceAlert alert)
        {
            await _context.ComplianceAlerts.AddAsync(alert);
        }

        public async Task UpdateAlertAsync(ComplianceAlert alert)
        {
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

        public async Task<ComplianceDashboard?> GetLatestDashboardAsync(Guid tenantId)
        {
            return await _context.Set<ComplianceDashboard>()
                .Where(d => d.TenantId == tenantId)
                .OrderByDescending(d => d.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        public async Task SaveDashboardAsync(ComplianceDashboard dashboard)
        {
            await _context.Set<ComplianceDashboard>().AddAsync(dashboard);
        }

        public async Task<Dictionary<string, int>> GetComplianceSummaryAsync()
        {
            return await _context.Assets
                .GroupBy(a => a.ComplianceStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        public async Task<double> GetAverageComplianceScoreAsync()
        {
            return await _context.ComplianceChecks.AverageAsync(c => (double?)c.Score) ?? 0;
        }

        public async Task<int> GetAssetsByStatusCountAsync(string status)
        {
            return await _context.Assets
                .CountAsync(a => a.ComplianceStatus == status);
        }

        public async Task<List<Asset>> GetNonCompliantAssetsAsync(int severity = 0)
        {
            return await _context.Assets
                .Where(a => a.ComplianceStatus == "Non-Compliant")
                .ToListAsync();
        }
    }
}
