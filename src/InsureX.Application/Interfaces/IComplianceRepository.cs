using InsureX.Domain.Entities;

namespace InsureX.Application.Interfaces;

public interface IComplianceRepository
{
    // Rules
    Task<List<ComplianceRule>> GetActiveRulesAsync();
    Task<ComplianceRule?> GetRuleByIdAsync(int id);
    Task AddRuleAsync(ComplianceRule rule);
    Task UpdateRuleAsync(ComplianceRule rule);

    // Checks
    Task<ComplianceCheck?> GetLatestCheckAsync(int assetId);
    Task AddCheckAsync(ComplianceCheck check);
    
    // Alerts
    Task<List<ComplianceAlert>> GetActiveAlertsAsync(int? assetId = null);
    Task<ComplianceAlert?> GetAlertByIdAsync(int id);
    Task AddAlertAsync(ComplianceAlert alert);
    Task UpdateAlertAsync(ComplianceAlert alert);
    Task<int> GetActiveAlertCountAsync();

    // History
    Task AddHistoryAsync(ComplianceHistory history);
    
    // Dashboard
    Task<Dictionary<string, int>> GetComplianceSummaryAsync();
    Task SaveChangesAsync();
}