using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces;

public interface IComplianceEngineService
{
    // Rule Management
    Task<List<ComplianceRuleDto>> GetActiveRulesAsync();
    Task<ComplianceRuleDto> CreateRuleAsync(CreateComplianceRuleDto dto);
    Task<ComplianceRuleDto> UpdateRuleAsync(int id, CreateComplianceRuleDto dto);
    Task<bool> DeleteRuleAsync(int id);
    Task<bool> ToggleRuleStatusAsync(int id, bool isActive);
    
    // Compliance Checks
    Task<ComplianceCheckResult> CheckAssetComplianceAsync(int assetId);
    Task<List<ComplianceCheckResult>> CheckAllAssetsAsync(bool force = false);
    Task<List<ComplianceCheckResult>> CheckAssetsNeedingUpdateAsync();
    
    // Status and History
    Task<ComplianceStatusDto> GetAssetComplianceStatusAsync(int assetId);
    Task<List<ComplianceHistory>> GetComplianceHistoryAsync(int assetId, int days = 90);
    Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30);
    
    // Alerts
    Task<List<ComplianceAlertDto>> GetActiveAlertsAsync(int? assetId = null);
    Task<ComplianceAlertDto> AcknowledgeAlertAsync(int alertId, string userId);
    Task<ComplianceAlertDto> ResolveAlertAsync(int alertId, string userId, string notes);
    Task<int> GetAlertCountAsync(int? assetId = null);
    
    // Dashboard
    Task<ComplianceDashboardDto> GetDashboardDataAsync();
    Task<ComplianceDashboardDto> RefreshDashboardAsync();
    
    // Batch Operations
    Task<int> UpdateExpiredRulesAsync();
    Task<int> ResolveStaleAlertsAsync(int daysOld);
    Task<Dictionary<string, int>> GetComplianceStatisticsAsync();
    
    // Reports
    Task<byte[]> GenerateComplianceReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Asset>> GetNonCompliantAssetsReportAsync(int minSeverity = 0);
}