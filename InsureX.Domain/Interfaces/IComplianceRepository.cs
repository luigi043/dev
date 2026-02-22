using System.Linq.Expressions;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces
{
    public interface IComplianceRepository : IRepository<ComplianceCheck>
    {
        // Rule Management - with int tenantId
        Task<List<ComplianceRule>> GetActiveRulesAsync(int tenantId);
        Task<ComplianceRule?> GetRuleByCodeAsync(string ruleCode, int tenantId);
        Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType, int tenantId);
        Task<List<ComplianceRule>> GetExpiringRulesAsync(int tenantId, int daysThreshold = 30);
        Task<bool> RuleExistsAsync(string ruleCode, int tenantId);
        Task<ComplianceRule?> GetRuleByIdAsync(int ruleId);
        Task<ComplianceRule> AddRuleAsync(ComplianceRule rule);
        Task UpdateRuleAsync(ComplianceRule rule);
        Task DeleteRuleAsync(int ruleId);

        // Compliance Checks - with int IDs
        Task<ComplianceCheck?> GetLatestCheckAsync(int assetId);
        Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30);
        Task<List<ComplianceCheck>> GetChecksByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
        Task<List<ComplianceCheck>> GetAssetsNeedingCheckAsync(int tenantId, int hoursThreshold = 24);
        Task<int> GetCheckCountAsync(int assetId, DateTime? since = null);
        Task<double> GetAverageScoreAsync(int assetId, int days = 90);
        Task<ComplianceCheck?> GetCheckByIdAsync(int checkId);

        // Compliance History - with int IDs
        Task<List<ComplianceHistory>> GetHistoryAsync(int assetId, int days = 90);
        Task<List<ComplianceHistory>> GetHistoryByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
        Task<ComplianceHistory?> GetLastStatusChangeAsync(int assetId);
        Task AddHistoryAsync(ComplianceHistory history);
        Task<List<ComplianceHistory>> GetAuditTrailAsync(int assetId, DateTime? from = null, DateTime? to = null);

        // Alert Management - with int IDs
        Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null, int? tenantId = null);
        Task<List<ComplianceAlert>> GetAlertsByAssetAsync(int assetId);
        Task<List<ComplianceAlert>> GetAlertsByStatusAsync(string status, int tenantId);
        Task<List<ComplianceAlert>> GetAlertsBySeverityAsync(int minSeverity, int tenantId);
        Task<List<ComplianceAlert>> GetOverdueAlertsAsync(int tenantId);
        Task<List<ComplianceAlert>> GetAlertsByTypeAsync(string alertType, int tenantId);
        Task<ComplianceAlert?> GetAlertByIdAsync(int alertId);
        Task AcknowledgeAlertAsync(int id, string userId, int tenantId);
        Task ResolveAlertAsync(int id, string resolution, string userId, int tenantId);
        Task<int> GetActiveAlertCountAsync(int? assetId = null, int? tenantId = null);
        Task<Dictionary<string, int>> GetAlertStatisticsAsync(int tenantId);
        Task<ComplianceAlert> AddAlertAsync(ComplianceAlert alert);
        Task UpdateAlertAsync(ComplianceAlert alert);

        // Dashboard & Analytics - with int tenantId
        Task<ComplianceDashboard?> GetLatestDashboardAsync(int tenantId);
        Task<List<ComplianceDashboard>> GetDashboardHistoryAsync(int tenantId, int days = 30);
        Task SaveDashboardAsync(ComplianceDashboard dashboard);
        Task<Dictionary<string, int>> GetComplianceSummaryAsync(int tenantId);
        Task<double> GetOverallComplianceRateAsync(int tenantId);
        Task<Dictionary<string, double>> GetComplianceByAssetTypeAsync(int tenantId);
        Task<Dictionary<string, int>> GetViolationsByRuleTypeAsync(int tenantId, int days = 30);
        Task<List<Asset>> GetNonCompliantAssetsAsync(int tenantId, int minSeverity = 0);
        Task<List<Asset>> GetCriticalAssetsAsync(int tenantId);
        Task<double> GetAverageResponseTimeAsync(int tenantId, int days = 30);
        Task<double> GetAverageResolutionTimeAsync(int tenantId, int days = 30);

        // Advanced Queries - with int IDs
        Task<IEnumerable<ComplianceRule>> EvaluateAssetRulesAsync(int assetId);
        Task<Dictionary<int, bool>> BatchCheckComplianceAsync(List<int> assetIds);
        Task<List<ComplianceCheck>> GetFailedChecksAsync(int tenantId, DateTime? since = null);
        Task<int> GetAssetsByStatusCountAsync(string status, int tenantId);

        // Reporting - with int tenantId
        Task<byte[]> GenerateComplianceReportAsync(int tenantId, DateTime startDate, DateTime endDate, string format = "pdf");
        Task<Dictionary<string, object>> GetComplianceMetricsAsync(int tenantId);
        Task<List<ComplianceHistory>> GetStatusChangeTimelineAsync(int assetId, int months = 6);
    }
}