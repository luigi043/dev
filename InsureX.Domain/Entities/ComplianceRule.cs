using InsureX.Domain.Entities;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;


namespace InsureX.Domain.Interfaces
{
    public interface IComplianceRepository : IRepository<ComplianceCheck>
    {
        // Rule Management
        Task<List<ComplianceRule>> GetActiveRulesAsync(Guid tenantId);
        Task<ComplianceRule?> GetRuleByCodeAsync(string ruleCode, Guid tenantId);
        Task<List<ComplianceRule>> GetRulesByTypeAsync(string ruleType, Guid tenantId);
        Task<ComplianceRule?> GetRuleByIdAsync(Guid id, Guid tenantId);
        Task<List<ComplianceRule>> GetRulesByPriorityAsync(Guid tenantId, int minPriority = 0);
        Task<List<ComplianceRule>> GetExpiringRulesAsync(Guid tenantId, int daysThreshold = 30);
        Task AddRuleAsync(ComplianceRule rule);
        Task UpdateRuleAsync(ComplianceRule rule);
        Task DeleteRuleAsync(Guid id, Guid tenantId);
        Task<bool> RuleExistsAsync(string ruleCode, Guid tenantId);

        // Compliance Checks
        Task<ComplianceCheck?> GetLatestCheckAsync(Guid assetId);
        Task<List<ComplianceCheck>> GetCheckHistoryAsync(Guid assetId, int days = 30);
        Task<List<ComplianceCheck>> GetChecksByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate);
        Task<List<ComplianceCheck>> GetAssetsNeedingCheckAsync(Guid tenantId, int hoursThreshold = 24);
        Task<ComplianceCheck> AddCheckAsync(ComplianceCheck check);
        Task UpdateCheckAsync(ComplianceCheck check);
        Task<int> GetCheckCountAsync(Guid assetId, DateTime? since = null);
        Task<double> GetAverageScoreAsync(Guid assetId, int days = 90);

        // Compliance History
        Task<List<ComplianceHistory>> GetHistoryAsync(Guid assetId, int days = 90);
        Task<List<ComplianceHistory>> GetHistoryByDateRangeAsync(Guid tenantId, DateTime startDate, DateTime endDate);
        Task<ComplianceHistory?> GetLastStatusChangeAsync(Guid assetId);
        Task AddHistoryAsync(ComplianceHistory history);
        Task<List<ComplianceHistory>> GetAuditTrailAsync(Guid assetId, DateTime? from = null, DateTime? to = null);

        // Alert Management
        Task<List<ComplianceAlert>> GetActiveAlertsAsync(Guid? assetId = null, Guid? tenantId = null);
        Task<List<ComplianceAlert>> GetAlertsByAssetAsync(Guid assetId);
        Task<ComplianceAlert?> GetAlertByIdAsync(Guid id, Guid tenantId);
        Task<List<ComplianceAlert>> GetAlertsByStatusAsync(string status, Guid tenantId);
        Task<List<ComplianceAlert>> GetAlertsBySeverityAsync(int minSeverity, Guid tenantId);
        Task<List<ComplianceAlert>> GetOverdueAlertsAsync(Guid tenantId);
        Task<List<ComplianceAlert>> GetAlertsByTypeAsync(string alertType, Guid tenantId);
        Task AddAlertAsync(ComplianceAlert alert);
        Task UpdateAlertAsync(ComplianceAlert alert);
        Task AcknowledgeAlertAsync(Guid id, string userId, Guid tenantId);
        Task ResolveAlertAsync(Guid id, string resolution, string userId, Guid tenantId);
        Task<int> GetActiveAlertCountAsync(Guid? assetId = null, Guid? tenantId = null);
        Task<Dictionary<string, int>> GetAlertStatisticsAsync(Guid tenantId);

        // Dashboard & Analytics
        Task<ComplianceDashboard?> GetLatestDashboardAsync(Guid tenantId);
        Task<List<ComplianceDashboard>> GetDashboardHistoryAsync(Guid tenantId, int days = 30);
        Task SaveDashboardAsync(ComplianceDashboard dashboard);
        Task<Dictionary<string, int>> GetComplianceSummaryAsync(Guid tenantId);
        Task<double> GetOverallComplianceRateAsync(Guid tenantId);
        Task<Dictionary<string, double>> GetComplianceByAssetTypeAsync(Guid tenantId);
        Task<Dictionary<string, int>> GetViolationsByRuleTypeAsync(Guid tenantId, int days = 30);
        Task<List<Asset>> GetNonCompliantAssetsAsync(Guid tenantId, int minSeverity = 0);
        Task<List<Asset>> GetCriticalAssetsAsync(Guid tenantId);
        Task<Dictionary<int, int>> GetAlertsBySeverityAsync(Guid tenantId);
        Task<double> GetAverageResponseTimeAsync(Guid tenantId, int days = 30);
        Task<double> GetAverageResolutionTimeAsync(Guid tenantId, int days = 30);

        // Advanced Queries
        Task<IEnumerable<ComplianceRule>> EvaluateAssetRulesAsync(Guid assetId);
        Task<Dictionary<Guid, bool>> BatchCheckComplianceAsync(List<Guid> assetIds);
        Task<List<ComplianceCheck>> GetFailedChecksAsync(Guid tenantId, DateTime? since = null);
        Task<int> GetAssetsByStatusCountAsync(string status, Guid tenantId);
        Task<List<Guid>> GetAssetsWithExpiringRulesAsync(Guid tenantId, int daysThreshold = 30);
        
        // Reporting
        Task<byte[]> GenerateComplianceReportAsync(Guid tenantId, DateTime startDate, DateTime endDate, string format = "pdf");
        Task<Dictionary<string, object>> GetComplianceMetricsAsync(Guid tenantId);
        Task<List<ComplianceHistory>> GetStatusChangeTimelineAsync(Guid assetId, int months = 6);
    }
}