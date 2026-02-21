using InsureX.Application.DTOs;
using InsureX.Domain.Interfaces;
namespace InsureX.Application.Interfaces;

public interface IComplianceEngineService
{
    Task<ComplianceDashboardDto> GetDashboardDataAsync();
    Task<ComplianceStatusDto> GetAssetComplianceStatusAsync(int assetId);
    Task<List<ComplianceAlertDto>> GetActiveAlertsAsync(int? assetId = null);
    Task<ComplianceAlertDto> AcknowledgeAlertAsync(int alertId);
    Task<ComplianceAlertDto> ResolveAlertAsync(int alertId, string notes);
    Task<ComplianceCheckResult> CheckAssetComplianceAsync(int assetId);
    Task<List<ComplianceCheckResult>> CheckAllAssetsAsync();
    Task<Dictionary<string, int>> GetComplianceStatisticsAsync();
}

public class ComplianceCheckResult
{
    public int AssetId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<ComplianceAlertDto> NewAlerts { get; set; } = new();
}