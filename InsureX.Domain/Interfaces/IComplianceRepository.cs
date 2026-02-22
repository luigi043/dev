using InsureX.Domain.Entities;
using System.Linq.Expressions;

namespace InsureX.Domain.Interfaces
{
    public interface IComplianceRepository : IRepository<ComplianceCheck>
    {
        Task<List<ComplianceRule>> GetActiveRulesAsync();
        Task<ComplianceRule?> GetRuleByCodeAsync(string ruleCode);
        Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType);
        Task<ComplianceRule?> GetRuleByIdAsync(int id);
        Task AddRuleAsync(ComplianceRule rule);
        Task UpdateRuleAsync(ComplianceRule rule);
        Task<ComplianceCheck?> GetLatestCheckAsync(int assetId);
        Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30);
        Task<List<ComplianceCheck>> GetAssetsNeedingCheckAsync(int hoursThreshold = 24);
        Task<List<ComplianceHistory>> GetHistoryAsync(int assetId, int days = 90);
        Task AddHistoryAsync(ComplianceHistory history);
        Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null);
        Task<List<ComplianceAlert>> GetAlertsByAssetAsync(int assetId);
        Task<ComplianceAlert?> GetAlertByIdAsync(int id);
        Task AddAlertAsync(ComplianceAlert alert);
        Task UpdateAlertAsync(ComplianceAlert alert);
        Task<int> GetActiveAlertCountAsync(int? assetId = null);
        Task<ComplianceDashboard?> GetLatestDashboardAsync(Guid tenantId);
        Task SaveDashboardAsync(ComplianceDashboard dashboard);
        Task<Dictionary<string, int>> GetComplianceSummaryAsync();
        Task<double> GetAverageComplianceScoreAsync();
        Task<int> GetAssetsByStatusCountAsync(string status);
        Task<List<Asset>> GetNonCompliantAssetsAsync(int severity = 0);
    }
}
