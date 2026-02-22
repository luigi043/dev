using InsureX.Domain.Entities;
using System.Linq.Expressions;

namespace InsureX.Domain.Interfaces
{
    public interface IComplianceRepository : IRepository<ComplianceResult>
    {
        // Rule Management
        Task<List<ComplianceRule>> GetActiveRulesAsync(Guid tenantId);
        Task<ComplianceRule?> GetRuleByCodeAsync(string ruleCode, Guid tenantId);
        Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType, Guid tenantId);
        Task<ComplianceRule?> GetRuleByIdAsync(int id, Guid tenantId);
        Task AddRuleAsync(ComplianceRule rule);
        Task UpdateRuleAsync(ComplianceRule rule);
        Task DeleteRuleAsync(int id, Guid tenantId);

        // Compliance Checks
        Task<ComplianceResult?> GetLatestCheckAsync(Guid assetId);
        Task<List<ComplianceResult>> GetCheckHistoryAsync(Guid assetId, int days = 30);
        Task<List<ComplianceResult>> GetAssetsNeedingCheckAsync(Guid tenantId, int hoursThreshold = 24);
        Task<ComplianceResult> EvaluateAssetAsync(Guid assetId);
        Task LogComplianceCheckAsync(Guid assetId, bool isCompliant, string? reason = null);

        // Compliance History
        Task<List<ComplianceHistory>> GetHistoryAsync(Guid assetId, int days = 90);
        Task AddHistoryAsync(ComplianceHistory history);

        // Alerts
        Task<List<ComplianceAlert>> GetActiveAlertsAsync(Guid? assetId = null, Guid? tenantId = null);
        Task<List<ComplianceAlert>> GetAlertsByAssetAsync(Guid assetId);
        Task<ComplianceAlert?> GetAlertByIdAsync(int id, Guid tenantId);
        Task AddAlertAsync(ComplianceAlert alert);
        Task UpdateAlertAsync(ComplianceAlert alert);
        Task ResolveAlertAsync(int id, string resolution, Guid tenantId);
        Task<int> GetActiveAlertCountAsync(Guid? assetId = null, Guid? tenantId = null);

        // Dashboard & Summary
        Task<ComplianceDashboard?> GetLatestDashboardAsync(Guid tenantId);
        Task SaveDashboardAsync(ComplianceDashboard dashboard);
        Task<Dictionary<string, int>> GetComplianceSummaryAsync(Guid tenantId);
        Task<double> GetAverageComplianceScoreAsync(Guid tenantId);
        Task<int> GetAssetsByStatusCountAsync(string status, Guid tenantId);
        Task<List<Asset>> GetNonCompliantAssetsAsync(Guid tenantId, int severity = 0);

        // Additional utility methods
        Task<bool> IsAssetCompliantAsync(Guid assetId);
        Task<int> GetTotalRulesCountAsync(Guid tenantId);
        Task<Dictionary<string, double>> GetRuleEffectivenessAsync(Guid tenantId, DateTime startDate, DateTime endDate);
    }
}