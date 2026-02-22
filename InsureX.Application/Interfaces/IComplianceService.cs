using InsureX.Application.DTOs;

namespace InsureX.Application.Interfaces
{
    public interface IComplianceService
    {
        // Basic compliance checks
        Task<ComplianceCheckResultDto> CheckAssetComplianceAsync(int assetId);
        Task<List<ComplianceCheckResultDto>> CheckAllAssetsAsync();
        
        // Compliance status
        Task<ComplianceStatusDto> GetAssetComplianceStatusAsync(int assetId);
        Task<ComplianceSummaryDto> GetTenantComplianceSummaryAsync(int tenantId);
        Task<List<ComplianceHistoryDto>> GetComplianceHistoryAsync(int assetId, int days = 30);
        
        // Alerts
        Task<List<ComplianceAlertDto>> GetActiveAlertsAsync(int tenantId);
        Task<int> GetAlertCountAsync(int tenantId);
        Task<ComplianceAlertDto> ResolveAlertAsync(int alertId, string resolvedBy);
        
        // Rules
        Task<List<ComplianceRuleDto>> GetApplicableRulesAsync(int assetId);
        Task<ComplianceRuleDto> GetRuleByIdAsync(int ruleId);
        
        // Reports
        Task<byte[]> GenerateComplianceReportAsync(int tenantId, DateTime fromDate, DateTime toDate);
        
        // Dashboard data
        Task<ComplianceDashboardDto> GetDashboardDataAsync(int tenantId);
    }
}