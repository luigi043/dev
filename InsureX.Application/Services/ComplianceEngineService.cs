using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsureX.Application.DTOs;
using InsureX.Application.Interfaces;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Entities;
using Mapster;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Extensions.Logging;

namespace InsureX.Application.Services;

public class ComplianceEngineService : IComplianceEngineService
{
    private readonly IComplianceRepository _complianceRepo;
    private readonly IAssetRepository _assetRepo;
    private readonly ITenantContext _tenantContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ComplianceEngineService> _logger;
    private readonly IScriptRuleEvaluator _scriptEvaluator;

    public ComplianceEngineService(
        IComplianceRepository complianceRepo,
        IAssetRepository assetRepo,
        ITenantContext tenantContext,
        INotificationService notificationService,
        ILogger<ComplianceEngineService> logger,
        IScriptRuleEvaluator scriptEvaluator)
    {
        _complianceRepo = complianceRepo;
        _assetRepo = assetRepo;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _logger = logger;
        _scriptEvaluator = scriptEvaluator;
    }

    public async Task<List<ComplianceRuleDto>> GetActiveRulesAsync()
    {
        var rules = await _complianceRepo.GetActiveRulesAsync();
        return rules.Adapt<List<ComplianceRuleDto>>();
    }

    public async Task<ComplianceRuleDto> CreateRuleAsync(CreateComplianceRuleDto dto)
    {
        var rule = dto.Adapt<ComplianceRule>();
        await _complianceRepo.AddRuleAsync(rule);
        return rule.Adapt<ComplianceRuleDto>();
    }

    public async Task<ComplianceRuleDto> UpdateRuleAsync(int id, CreateComplianceRuleDto dto)
    {
        var existing = await _complianceRepo.GetRuleByIdAsync(id);
        if (existing is null) throw new KeyNotFoundException("Rule not found");

        dto.Adapt(existing);
        await _complianceRepo.UpdateRuleAsync(existing);
        return existing.Adapt<ComplianceRuleDto>();
    }

    public async Task<bool> DeleteRuleAsync(int id)
    {
        var existing = await _complianceRepo.GetRuleByIdAsync(id);
        if (existing is null) return false;
        existing.IsActive = false;
        await _complianceRepo.UpdateRuleAsync(existing);
        return true;
    }

    public async Task<bool> ToggleRuleStatusAsync(int id, bool isActive)
    {
        var existing = await _complianceRepo.GetRuleByIdAsync(id);
        if (existing is null) return false;
        existing.IsActive = isActive;
        await _complianceRepo.UpdateRuleAsync(existing);
        return true;
    }

    public async Task<ComplianceCheckResultDto> CheckAssetComplianceAsync(int assetId)
    {
        var asset = (await _assetRepo.GetByIdAsync(assetId)) as Asset;
        if (asset is null)
            throw new KeyNotFoundException("Asset not found");

        var rules = await _complianceRepo.GetActiveRulesAsync();
        var findings = new List<string>();
        var score = 100;

        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            // Simple rule evaluation examples
            if (rule.RuleType == "Inspection" && rule.DaysToExpiry.HasValue)
            {
                var lastInspection = asset.LastInspectionDate ?? asset.CreatedAt;
                var nextDue = lastInspection.AddDays(rule.DaysToExpiry.Value);
                if (DateTime.UtcNow > nextDue)
                {
                    findings.Add($"Inspection overdue by {(DateTime.UtcNow - nextDue).Days} days (rule {rule.RuleCode})");
                    score -= Math.Min(30, rule.Severity * 5);
                }
            }

            if (rule.RuleType == "Policy")
            {
                if (rule.MinValue.HasValue && asset.InsuredValue.HasValue && asset.InsuredValue < rule.MinValue)
                {
                    findings.Add($"Insured value below minimum (rule {rule.RuleCode})");
                    score -= Math.Min(20, rule.Severity * 3);
                }
                if (rule.MaxValue.HasValue && asset.InsuredValue.HasValue && asset.InsuredValue > rule.MaxValue)
                {
                    findings.Add($"Insured value above maximum (rule {rule.RuleCode})");
                    score -= Math.Min(20, rule.Severity * 3);
                }
            }

            // Evaluate custom script if provided
            if (!string.IsNullOrWhiteSpace(rule.CustomScript) && _scriptEvaluator != null)
            {
                try
                {
                    var scriptResult = await _scriptEvaluator.EvaluateAsync(rule, asset);
                    if (!scriptResult)
                    {
                        findings.Add($"Rule {rule.RuleCode} failed script evaluation");
                        score -= Math.Min(30, rule.Severity * 5);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error evaluating script for rule {RuleId}", rule.Id);
                }
            }
        }

        score = Math.Clamp(score, 0, 100);

        var check = new ComplianceCheck
        {
            AssetId = assetId,
            CheckDate = DateTime.UtcNow,
            Status = score >= 80 ? "Compliant" : (score >= 50 ? "Warning" : "Non-Compliant"),
            Score = score,
            Findings = System.Text.Json.JsonSerializer.Serialize(findings),
            CheckedBy = "system",
            IsAutomatic = true,
            NextCheckDate = DateTime.UtcNow.AddDays(30)
        };

        await _complianceRepo.AddAsync(check);
        await _complianceRepo.SaveChangesAsync();

        var dto = check.Adapt<ComplianceCheckResultDto>();
        dto.CheckedAt = check.CheckDate;
        dto.Findings = string.Join("; ", findings);
        return dto;
    }

    public async Task<List<ComplianceCheckResultDto>> CheckAllAssetsAsync(bool force = false)
    {
        var checks = await _complianceRepo.GetAssetsNeedingCheckAsync(int.MaxValue);
        var results = new List<ComplianceCheckResultDto>();
        foreach (var c in checks)
        {
            var r = c.Adapt<ComplianceCheckResultDto>();
            r.CheckedAt = c.CheckDate;
            results.Add(r);
        }
        return results;
    }

    public async Task<List<ComplianceCheckResultDto>> CheckAssetsNeedingUpdateAsync()
    {
        var toCheck = await _complianceRepo.GetAssetsNeedingCheckAsync();
        var results = new List<ComplianceCheckResultDto>();
        foreach (var c in toCheck)
        {
            var r = c.Adapt<ComplianceCheckResultDto>();
            r.CheckedAt = c.CheckDate;
            results.Add(r);
        }
        return results;
    }

    public async Task<ComplianceStatusDto> GetAssetComplianceStatusAsync(int assetId)
    {
        var latest = await _complianceRepo.GetLatestCheckAsync(assetId);
        if (latest is null)
            return new ComplianceStatusDto { AssetId = assetId, Status = "Unknown", Score = 0, LastChecked = DateTime.MinValue };

        var dto = latest.Adapt<ComplianceStatusDto>();
        dto.LastChecked = latest.CheckDate;
        return dto;
    }

    public async Task<List<ComplianceHistoryDto>> GetComplianceHistoryAsync(int assetId, int days = 90)
    {
        var history = await _complianceRepo.GetHistoryAsync(assetId, days);
        return history.Adapt<List<ComplianceHistoryDto>>();
    }

    public async Task<List<ComplianceCheckDto>> GetCheckHistoryAsync(int assetId, int days = 30)
    {
        var checks = await _complianceRepo.GetCheckHistoryAsync(assetId, days);
        return checks.Adapt<List<ComplianceCheckDto>>();
    }

    public async Task<List<ComplianceAlertDto>> GetActiveAlertsAsync(int? assetId = null)
    {
        var alerts = await _complianceRepo.GetActiveAlertsAsync(assetId);
        return alerts.Adapt<List<ComplianceAlertDto>>();
    }

    public async Task<ComplianceAlertDto> AcknowledgeAlertAsync(int alertId, string userId)
    {
        var alert = await _complianceRepo.GetAlertByIdAsync(alertId);
        if (alert is null) throw new KeyNotFoundException("Alert not found");
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = userId;
        alert.Status = "Acknowledged";
        await _complianceRepo.UpdateAlertAsync(alert);
        return alert.Adapt<ComplianceAlertDto>();
    }

    public async Task<ComplianceAlertDto> ResolveAlertAsync(int alertId, string userId, string notes)
    {
        var alert = await _complianceRepo.GetAlertByIdAsync(alertId);
        if (alert is null) throw new KeyNotFoundException("Alert not found");
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedBy = userId;
        alert.ResolutionNotes = notes;
        alert.Status = "Resolved";
        await _complianceRepo.UpdateAlertAsync(alert);
        return alert.Adapt<ComplianceAlertDto>();
    }

    public Task<int> GetAlertCountAsync(int? assetId = null)
        => _complianceRepo.GetActiveAlertCountAsync(assetId);

    public async Task<ComplianceDashboardDto> GetDashboardDataAsync()
    {
        var tenantId = _tenantContext.GetCurrentTenantId() ?? Guid.Empty;
        var dash = await _complianceRepo.GetLatestDashboardAsync(tenantId);
        if (dash is null) return new ComplianceDashboardDto();
        return dash.Adapt<ComplianceDashboardDto>();
    }

    public async Task<ComplianceDashboardDto> RefreshDashboardAsync()
    {
        var stats = await _complianceRepo.GetComplianceSummaryAsync();
        var overall = await _complianceRepo.GetAverageComplianceScoreAsync();
        var dto = new ComplianceDashboardDto
        {
            TotalAssets = stats.GetValueOrDefault("TotalAssets", 0),
            CompliantAssets = stats.GetValueOrDefault("Compliant", 0),
            NonCompliantAssets = stats.GetValueOrDefault("NonCompliant", 0),
            WarningAssets = stats.GetValueOrDefault("Warning", 0),
            OverallComplianceRate = overall,
            ActiveAlerts = stats.GetValueOrDefault("ActiveAlerts", 0),
            RecentAlerts = (await _complianceRepo.GetActiveAlertsAsync(null)).Adapt<List<ComplianceAlertDto>>(),
            TopIssues = new List<TopIssueDto>()
        };

        var tenantId = _tenantContext.GetCurrentTenantId() ?? Guid.Empty;
        var db = new ComplianceDashboard
        {
            TenantId = tenantId,
            SnapshotDate = DateTime.UtcNow,
            TotalAssets = dto.TotalAssets,
            CompliantAssets = dto.CompliantAssets,
            NonCompliantAssets = dto.NonCompliantAssets,
            WarningAssets = dto.WarningAssets,
            OverallComplianceRate = dto.OverallComplianceRate,
            ActiveAlerts = dto.ActiveAlerts,
            TopIssues = null
        };

        await _complianceRepo.SaveDashboardAsync(db);
        return dto;
    }

    public async Task<int> UpdateExpiredRulesAsync()
    {
        var rules = await _complianceRepo.GetActiveRulesAsync();
        var now = DateTime.UtcNow;
        var count = 0;
        foreach (var r in rules)
        {
            if (r.EffectiveTo.HasValue && r.EffectiveTo.Value < now)
            {
                r.IsActive = false;
                await _complianceRepo.UpdateRuleAsync(r);
                count++;
            }
        }
        return count;
    }

    public async Task<int> ResolveStaleAlertsAsync(int daysOld)
    {
        var alerts = await _complianceRepo.GetActiveAlertsAsync(null);
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        var count = 0;
        foreach (var a in alerts)
        {
            if (a.CreatedAt < cutoff)
            {
                a.Status = "Resolved";
                a.ResolvedAt = DateTime.UtcNow;
                a.ResolvedBy = "system";
                await _complianceRepo.UpdateAlertAsync(a);
                count++;
            }
        }
        return count;
    }

    public Task<Dictionary<string, int>> GetComplianceStatisticsAsync()
        => _complianceRepo.GetComplianceSummaryAsync();

    public async Task<byte[]> GenerateComplianceReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var checks = await _complianceRepo.GetCheckHistoryAsync(0, 365);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("ComplianceReport");

        // Header
        ws.Cells[1, 1].Value = "AssetId";
        ws.Cells[1, 2].Value = "AssetTag";
        ws.Cells[1, 3].Value = "Status";
        ws.Cells[1, 4].Value = "Score";
        ws.Cells[1, 5].Value = "CheckDate";
        ws.Cells[1, 6].Value = "Findings";

        using (var rng = ws.Cells[1, 1, 1, 6])
        {
            rng.Style.Font.Bold = true;
            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        var row = 2;
        foreach (var c in checks)
        {
            if (fromDate.HasValue && c.CheckDate < fromDate.Value) continue;
            if (toDate.HasValue && c.CheckDate > toDate.Value) continue;

            var asset = await _assetRepo.GetByIdAsync(c.AssetId) as Asset;
            ws.Cells[row, 1].Value = c.AssetId;
            ws.Cells[row, 2].Value = asset?.AssetTag ?? string.Empty;
            ws.Cells[row, 3].Value = c.Status;
            ws.Cells[row, 4].Value = c.Score;
            ws.Cells[row, 5].Value = c.CheckDate;
            ws.Cells[row, 6].Value = c.Findings;
            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }

    public Task<List<AssetDto>> GetNonCompliantAssetsReportAsync(int minSeverity = 0)
        => _complianceRepo.GetNonCompliantAssetsAsync(minSeverity).ContinueWith(t => t.Result.Adapt<List<AssetDto>>());
}
