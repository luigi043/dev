using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Exceptions;
using Mapster;

namespace InsureX.Application.Services;

public class ComplianceEngineService : IComplianceEngineService
{
    private readonly IComplianceRepository _complianceRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IPolicyRepository _policyRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ComplianceEngineService> _logger;

    public ComplianceEngineService(
        IComplianceRepository complianceRepository,
        IAssetRepository assetRepository,
        IPolicyRepository policyRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        ILogger<ComplianceEngineService> logger)
    {
        _complianceRepository = complianceRepository;
        _assetRepository = assetRepository;
        _policyRepository = policyRepository;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _logger = logger;
    }

    // Rule Management
    public async Task<List<ComplianceRuleDto>> GetActiveRulesAsync()
    {
        var rules = await _complianceRepository.GetActiveRulesAsync();
        return rules.Adapt<List<ComplianceRuleDto>>();
    }

    public async Task<ComplianceRuleDto> CreateRuleAsync(CreateComplianceRuleDto dto)
    {
        // Check for duplicate rule code
        var existing = await _complianceRepository.GetRuleByCodeAsync(dto.RuleCode);
        if (existing != null)
        {
            throw new DomainException($"Rule with code {dto.RuleCode} already exists");
        }

        var rule = dto.Adapt<ComplianceRule>();
        rule.TenantId = _tenantContext.GetCurrentTenantId() 
            ?? throw new UnauthorizedAccessException("No tenant context");
        rule.CreatedBy = _currentUserService.GetCurrentUserId() ?? "system";
        rule.CreatedAt = DateTime.UtcNow;

        // Serialize arrays to JSON for storage
        rule.ApplicablePolicyTypes = JsonSerializer.Serialize(dto.ApplicablePolicyTypes);
        rule.ApplicableAssetTypes = JsonSerializer.Serialize(dto.ApplicableAssetTypes);

        await _complianceRepository.AddRuleAsync(rule);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Compliance rule created: {RuleCode} - {RuleName}", 
            rule.RuleCode, rule.RuleName);

        return rule.Adapt<ComplianceRuleDto>();
    }

    public async Task<ComplianceRuleDto> UpdateRuleAsync(int id, CreateComplianceRuleDto dto)
    {
        var rule = await _complianceRepository.GetRuleByIdAsync(id);
        if (rule == null)
        {
            throw new DomainException($"Rule with ID {id} not found");
        }

        // Check if code changed and is unique
        if (rule.RuleCode != dto.RuleCode)
        {
            var existing = await _complianceRepository.GetRuleByCodeAsync(dto.RuleCode);
            if (existing != null)
            {
                throw new DomainException($"Rule with code {dto.RuleCode} already exists");
            }
        }

        dto.Adapt(rule);
        rule.UpdatedAt = DateTime.UtcNow;
        rule.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";
        rule.ApplicablePolicyTypes = JsonSerializer.Serialize(dto.ApplicablePolicyTypes);
        rule.ApplicableAssetTypes = JsonSerializer.Serialize(dto.ApplicableAssetTypes);

        await _complianceRepository.UpdateRuleAsync(rule);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Compliance rule updated: {RuleCode}", rule.RuleCode);

        return rule.Adapt<ComplianceRuleDto>();
    }

    public async Task<bool> DeleteRuleAsync(int id)
    {
        var rule = await _complianceRepository.GetRuleByIdAsync(id);
        if (rule == null)
        {
            return false;
        }

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        rule.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

        await _complianceRepository.UpdateRuleAsync(rule);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Compliance rule deactivated: {RuleCode}", rule.RuleCode);

        return true;
    }

    public async Task<bool> ToggleRuleStatusAsync(int id, bool isActive)
    {
        var rule = await _complianceRepository.GetRuleByIdAsync(id);
        if (rule == null)
        {
            return false;
        }

        rule.IsActive = isActive;
        rule.UpdatedAt = DateTime.UtcNow;
        rule.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

        await _complianceRepository.UpdateRuleAsync(rule);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Rule {RuleCode} status toggled to {IsActive}", 
            rule.RuleCode, isActive);

        return true;
    }

    // Compliance Checks
    public async Task<ComplianceCheckResult> CheckAssetComplianceAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            throw new DomainException($"Asset with ID {assetId} not found");
        }

        var policies = await _policyRepository.GetByAssetIdAsync(assetId);
        var rules = await _complianceRepository.GetActiveRulesAsync();
        
        var result = new ComplianceCheckResult
        {
            AssetId = assetId,
            RuleResults = new List<RuleEvaluationResult>(),
            NewAlerts = new List<ComplianceAlertDto>()
        };

        int totalScore = 0;
        int applicableRules = 0;

        foreach (var rule in rules)
        {
            // Check if rule applies to this asset
            if (!IsRuleApplicable(rule, asset, policies))
            {
                continue;
            }

            var ruleResult = await EvaluateRule(rule, asset, policies);
            result.RuleResults.Add(ruleResult);
            
            applicableRules++;
            if (ruleResult.IsCompliant)
            {
                totalScore += 100;
            }
            else
            {
                // Create alert for non-compliance
                var alert = await CreateAlertFromRule(rule, asset, ruleResult.Finding);
                result.NewAlerts.Add(alert.Adapt<ComplianceAlertDto>());
            }
        }

        // Calculate overall score
        result.Score = applicableRules > 0 ? totalScore / applicableRules : 100;
        
        // Determine overall status
        result.Status = result.Score >= 90 ? "Compliant" :
                       result.Score >= 70 ? "Warning" : "Non-Compliant";

        // Save compliance check
        var check = new ComplianceCheck
        {
            AssetId = assetId,
            CheckDate = DateTime.UtcNow,
            Status = result.Status,
            Score = result.Score,
            Findings = JsonSerializer.Serialize(result.RuleResults),
            CheckedBy = _currentUserService.GetCurrentUserId() ?? "system",
            IsAutomatic = true,
            NextCheckDate = DateTime.UtcNow.AddDays(1) // Check again tomorrow
        };

        await _complianceRepository.AddAsync(check);
        
        // Update asset compliance status if changed
        if (asset.ComplianceStatus != result.Status)
        {
            // Record history
            var history = new ComplianceHistory
            {
                AssetId = assetId,
                ChangeDate = DateTime.UtcNow,
                FromStatus = asset.ComplianceStatus ?? "Unknown",
                ToStatus = result.Status,
                FromScore = asset.ComplianceScore ?? 0,
                ToScore = result.Score,
                Reason = "Automatic compliance check",
                TriggeredBy = "system"
            };
            await _complianceRepository.AddHistoryAsync(history);

            // Update asset
            asset.ComplianceStatus = result.Status;
            asset.ComplianceScore = result.Score;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = "system";
            await _assetRepository.UpdateAsync(asset);
        }

        await _complianceRepository.SaveChangesAsync();

        // Send notifications for critical alerts
        foreach (var alert in result.NewAlerts.Where(a => a.Severity >= 3))
        {
            await _notificationService.SendEmailAsync(
                _currentUserService.GetCurrentUserEmail() ?? "",
                $"Compliance Alert: {alert.Title}",
                alert.Description
            );
        }

        _logger.LogInformation("Compliance check completed for Asset {AssetId}: {Status} ({Score}%)", 
            assetId, result.Status, result.Score);

        return result;
    }

    public async Task<List<ComplianceCheckResult>> CheckAllAssetsAsync(bool force = false)
    {
        var results = new List<ComplianceCheckResult>();
        var assets = await _assetRepository.GetAllAsync();

        foreach (var asset in assets)
        {
            if (asset.Status == "Deleted") continue;

            try
            {
                var result = await CheckAssetComplianceAsync(asset.Id);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compliance for Asset {AssetId}", asset.Id);
            }
        }

        // Refresh dashboard after batch check
        await RefreshDashboardAsync();

        return results;
    }

    public async Task<List<ComplianceCheckResult>> CheckAssetsNeedingUpdateAsync()
    {
        var results = new List<ComplianceCheckResult>();
        var assetsNeedingCheck = await _complianceRepository.GetAssetsNeedingCheckAsync();

        foreach (var check in assetsNeedingCheck)
        {
            try
            {
                var result = await CheckAssetComplianceAsync(check.AssetId);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compliance for Asset {AssetId}", check.AssetId);
            }
        }

        return results;
    }

    // Status and History
    public async Task<ComplianceStatusDto> GetAssetComplianceStatusAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null)
        {
            throw new DomainException($"Asset with ID {assetId} not found");
        }

        var latestCheck = await _complianceRepository.GetLatestCheckAsync(assetId);
        var activeAlerts = await _complianceRepository.GetActiveAlertsAsync(assetId);
        var findings = latestCheck?.Findings != null 
            ? JsonSerializer.Deserialize<List<RuleEvaluationResult>>(latestCheck.Findings)
            : new List<RuleEvaluationResult>();

        return new ComplianceStatusDto
        {
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            Status = asset.ComplianceStatus ?? "Unknown",
            Score = asset.ComplianceScore ?? 0,
            LastChecked = latestCheck?.CheckDate ?? DateTime.MinValue,
            NextCheck = latestCheck?.NextCheckDate,
            Findings = findings?.Adapt<List<ComplianceFindingDto>>() ?? new(),
            ActiveAlerts = activeAlerts.Adapt<List<ComplianceAlertDto>>()
        };
    }

    public async Task<List<ComplianceHistory>> GetComplianceHistoryAsync(int assetId, int days = 90)
    {
        return await _complianceRepository.GetHistoryAsync(assetId, days);
    }

    public async Task<List<ComplianceCheck>> GetCheckHistoryAsync(int assetId, int days = 30)
    {
        return await _complianceRepository.GetCheckHistoryAsync(assetId, days);
    }

    // Alerts
    public async Task<List<ComplianceAlertDto>> GetActiveAlertsAsync(int? assetId = null)
    {
        var alerts = await _complianceRepository.GetActiveAlertsAsync(assetId);
        return alerts.Adapt<List<ComplianceAlertDto>>();
    }

    public async Task<ComplianceAlertDto> AcknowledgeAlertAsync(int alertId, string userId)
    {
        var alert = await _complianceRepository.GetAlertByIdAsync(alertId);
        if (alert == null)
        {
            throw new DomainException($"Alert with ID {alertId} not found");
        }

        alert.Status = "Acknowledged";
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = userId;
        alert.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAlertAsync(alert);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Alert {AlertId} acknowledged by {User}", alertId, userId);

        return alert.Adapt<ComplianceAlertDto>();
    }

    public async Task<ComplianceAlertDto> ResolveAlertAsync(int alertId, string userId, string notes)
    {
        var alert = await _complianceRepository.GetAlertByIdAsync(alertId);
        if (alert == null)
        {
            throw new DomainException($"Alert with ID {alertId} not found");
        }

        alert.Status = "Resolved";
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedBy = userId;
        alert.ResolutionNotes = notes;
        alert.UpdatedAt = DateTime.UtcNow;

        await _complianceRepository.UpdateAlertAsync(alert);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Alert {AlertId} resolved by {User}", alertId, userId);

        return alert.Adapt<ComplianceAlertDto>();
    }

    public async Task<int> GetAlertCountAsync(int? assetId = null)
    {
        return await _complianceRepository.GetActiveAlertCountAsync(assetId);
    }

    // Dashboard
    public async Task<ComplianceDashboardDto> GetDashboardDataAsync()
    {
        var tenantId = _tenantContext.GetCurrentTenantId() 
            ?? throw new UnauthorizedAccessException("No tenant context");

        // Try to get cached dashboard
        var cached = await _complianceRepository.GetLatestDashboardAsync(tenantId);
        if (cached != null && cached.SnapshotDate > DateTime.UtcNow.AddHours(-1))
        {
            return DeserializeDashboard(cached);
        }

        // Generate fresh dashboard
        return await RefreshDashboardAsync();
    }

    public async Task<ComplianceDashboardDto> RefreshDashboardAsync()
    {
        var tenantId = _tenantContext.GetCurrentTenantId() 
            ?? throw new UnauthorizedAccessException("No tenant context");

        var assets = await _assetRepository.GetAllAsync();
        var activeAlerts = await _complianceRepository.GetActiveAlertsAsync();
        
        var dashboard = new ComplianceDashboard
        {
            TenantId = tenantId,
            SnapshotDate = DateTime.UtcNow,
            TotalAssets = assets.Count,
            CompliantAssets = assets.Count(a => a.ComplianceStatus == "Compliant"),
            NonCompliantAssets = assets.Count(a => a.ComplianceStatus == "Non-Compliant"),
            WarningAssets = assets.Count(a => a.ComplianceStatus == "Warning"),
            CriticalAssets = assets.Count(a => a.ComplianceStatus == "Critical"),
            OverallComplianceRate = assets.Any() 
                ? Math.Round(assets.Average(a => a.ComplianceScore ?? 0), 2) 
                : 0,
            ActiveAlerts = activeAlerts.Count,
            OverdueActions = activeAlerts.Count(a => a.DueDate < DateTime.UtcNow)
        };

        // Generate asset type breakdown
        var assetTypeBreakdown = assets
            .GroupBy(a => a.AssetType ?? "Other")
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionary(g => g.Type, g => g.Count);
        dashboard.AssetTypeBreakdown = JsonSerializer.Serialize(assetTypeBreakdown);

        // Generate trend data (last 30 days)
        var trendData = await GenerateTrendDataAsync();
        dashboard.TrendData = JsonSerializer.Serialize(trendData);

        // Get top issues
        var topIssues = await GetTopIssuesAsync();
        dashboard.TopIssues = JsonSerializer.Serialize(topIssues);

        await _complianceRepository.SaveDashboardAsync(dashboard);
        await _complianceRepository.SaveChangesAsync();

        _logger.LogInformation("Compliance dashboard refreshed for tenant {TenantId}", tenantId);

        return DeserializeDashboard(dashboard);
    }

    // Batch Operations
    public async Task<int> UpdateExpiredRulesAsync()
    {
        var rules = await _complianceRepository.GetActiveRulesAsync();
        var now = DateTime.UtcNow;
        var updated = 0;

        foreach (var rule in rules)
        {
            if (rule.EffectiveTo < now)
            {
                rule.IsActive = false;
                rule.UpdatedAt = now;
                rule.UpdatedBy = "system";
                await _complianceRepository.UpdateRuleAsync(rule);
                updated++;
            }
        }

        await _complianceRepository.SaveChangesAsync();
        return updated;
    }

    public async Task<int> ResolveStaleAlertsAsync(int daysOld)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        var staleAlerts = await _complianceRepository.GetActiveAlertsAsync();
        
        staleAlerts = staleAlerts
            .Where(a => a.CreatedAt < cutoff && a.Status != "Resolved")
            .ToList();

        foreach (var alert in staleAlerts)
        {
            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolvedBy = "system";
            alert.ResolutionNotes = "Automatically resolved due to age";
            await _complianceRepository.UpdateAlertAsync(alert);
        }

        await _complianceRepository.SaveChangesAsync();
        return staleAlerts.Count;
    }

    public async Task<Dictionary<string, int>> GetComplianceStatisticsAsync()
    {
        return await _complianceRepository.GetComplianceSummaryAsync();
    }

    // Reports
    public async Task<byte[]> GenerateComplianceReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddMonths(-1);
        toDate ??= DateTime.UtcNow;

        var assets = await _assetRepository.GetAllAsync();
        var history = new List<ComplianceHistory>();
        
        foreach (var asset in assets)
        {
            var assetHistory = await _complianceRepository.GetHistoryAsync(asset.Id, 30);
            history.AddRange(assetHistory);
        }

        // Generate CSV report
        var csv = new System.Text.StringBuilder();
        
        // Headers
        csv.AppendLine("Asset ID,Asset Tag,Status,Score,Date,Change Type,Reason");
        
        // Data
        foreach (var h in history.OrderByDescending(h => h.ChangeDate))
        {
            var asset = assets.FirstOrDefault(a => a.Id == h.AssetId);
            csv.AppendLine($"{h.AssetId},{asset?.AssetTag},{h.ToStatus},{h.ToScore},{h.ChangeDate:yyyy-MM-dd},{h.FromStatus}->{h.ToStatus},{h.Reason}");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<List<Asset>> GetNonCompliantAssetsReportAsync(int minSeverity = 0)
    {
        return await _complianceRepository.GetNonCompliantAssetsAsync(minSeverity);
    }

    // Private helper methods
    private bool IsRuleApplicable(ComplianceRule rule, Asset asset, List<Policy> policies)
    {
        // Check asset type applicability
        if (!string.IsNullOrEmpty(rule.ApplicableAssetTypes))
        {
            var types = JsonSerializer.Deserialize<string[]>(rule.ApplicableAssetTypes);
            if (types?.Any() == true && !types.Contains(asset.AssetType))
            {
                return false;
            }
        }

        // Check policy type applicability
        if (!string.IsNullOrEmpty(rule.ApplicablePolicyTypes) && policies.Any())
        {
            var types = JsonSerializer.Deserialize<string[]>(rule.ApplicablePolicyTypes);
            if (types?.Any() == true && !policies.Any(p => types.Contains(p.PolicyType)))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<RuleEvaluationResult> EvaluateRule(ComplianceRule rule, Asset asset, List<Policy> policies)
    {
        var result = new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            Severity = rule.Severity
        };

        try
        {
            switch (rule.RuleType)
            {
                case "Policy":
                    result = await EvaluatePolicyRule(rule, asset, policies);
                    break;
                case "Payment":
                    result = await EvaluatePaymentRule(rule, asset, policies);
                    break;
                case "Inspection":
                    result = await EvaluateInspectionRule(rule, asset);
                    break;
                case "Documentation":
                    result = await EvaluateDocumentationRule(rule, asset);
                    break;
                case "Custom":
                    result = await EvaluateCustomRule(rule, asset, policies);
                    break;
                default:
                    result.IsCompliant = true;
                    result.Finding = "Rule type not implemented";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule {RuleCode}", rule.RuleCode);
            result.IsCompliant = false;
            result.Finding = $"Error evaluating rule: {ex.Message}";
        }

        return result;
    }

    private async Task<RuleEvaluationResult> EvaluatePolicyRule(ComplianceRule rule, Asset asset, List<Policy> policies)
    {
        var result = new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            Severity = rule.Severity
        };

        // Check if asset has any active policy
        var activePolicies = policies.Where(p => 
            p.Status == "Active" && 
            p.EndDate >= DateTime.UtcNow).ToList();

        if (!activePolicies.Any())
        {
            result.IsCompliant = false;
            result.Finding = "No active policies found for this asset";
            result.Recommendation = "Purchase a new policy immediately";
            return result;
        }

        // Check if policy expires soon
        if (rule.DaysToExpiry.HasValue)
        {
            var expiringSoon = activePolicies.Any(p => 
                p.EndDate <= DateTime.UtcNow.AddDays(rule.DaysToExpiry.Value) &&
                p.EndDate >= DateTime.UtcNow);

            if (expiringSoon)
            {
                result.IsCompliant = false;
                result.Finding = $"Policy expires within {rule.DaysToExpiry} days";
                result.Recommendation = "Renew policy before expiration";
                return result;
            }
        }

        result.IsCompliant = true;
        result.Finding = "Asset has valid active policies";
        
        return await Task.FromResult(result);
    }

    private async Task<RuleEvaluationResult> EvaluatePaymentRule(ComplianceRule rule, Asset asset, List<Policy> policies)
    {
        var result = new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            Severity = rule.Severity
        };

        var activePolicies = policies.Where(p => p.Status == "Active").ToList();
        
        if (!activePolicies.Any())
        {
            result.IsCompliant = false;
            result.Finding = "No active policies to check payment status";
            return result;
        }

        var overduePolicies = activePolicies.Where(p => p.PaymentStatus == "Overdue").ToList();
        
        if (overduePolicies.Any())
        {
            result.IsCompliant = false;
            result.Finding = $"Payment overdue for policies: {string.Join(", ", overduePolicies.Select(p => p.PolicyNumber))}";
            result.Recommendation = "Process pending payments immediately to avoid policy cancellation";
        }
        else
        {
            result.IsCompliant = true;
            result.Finding = "All payments are up to date";
        }

        return await Task.FromResult(result);
    }

    private async Task<RuleEvaluationResult> EvaluateInspectionRule(ComplianceRule rule, Asset asset)
    {
        var result = new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            Severity = rule.Severity
        };

        if (!asset.LastInspectionDate.HasValue)
        {
            result.IsCompliant = false;
            result.Finding = "Asset has never been inspected";
            result.Recommendation = "Schedule initial inspection";
            return result;
        }

        var daysSinceInspection = (DateTime.UtcNow - asset.LastInspectionDate.Value).Days;
        
        if (rule.MaxValue.HasValue && daysSinceInspection > rule.MaxValue.Value)
        {
            result.IsCompliant = false;
            result.Finding = $"Last inspection was {daysSinceInspection} days ago (exceeds {rule.MaxValue} days)";
            result.Recommendation = "Schedule new inspection";
        }
        else
        {
            result.IsCompliant = true;
            result.Finding = $"Last inspection was {daysSinceInspection} days ago - within required interval";
        }

        return await Task.FromResult(result);
    }

    private async Task<RuleEvaluationResult> EvaluateDocumentationRule(ComplianceRule rule, Asset asset)
    {
        var result = new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            Severity = rule.Severity
        };

        // Check if asset has required documents
        var missingDocs = new List<string>();

        if (string.IsNullOrEmpty(asset.VIN))
            missingDocs.Add("VIN");
        
        if (asset.PurchaseDate == null)
            missingDocs.Add("Purchase Date");

        if (asset.InsuredValue == null || asset.InsuredValue == 0)
            missingDocs.Add("Insured Value");

        if (missingDocs.Any())
        {
            result.IsCompliant = false;
            result.Finding = $"Missing required documentation: {string.Join(", ", missingDocs)}";
            result.Recommendation = "Complete all required asset information";
        }
        else
        {
            result.IsCompliant = true;
            result.Finding = "All required documentation is present";
        }

        return await Task.FromResult(result);
    }

    private async Task<RuleEvaluationResult> EvaluateCustomRule(ComplianceRule rule, Asset asset, List<Policy> policies)
    {
        // For custom rules, you could implement script evaluation
        // This is a placeholder for complex logic
        return new RuleEvaluationResult
        {
            RuleId = rule.Id,
            RuleName = rule.RuleName,
            IsCompliant = true,
            Finding = "Custom rule evaluation not implemented",
            Severity = rule.Severity
        };
    }

    private async Task<ComplianceAlert> CreateAlertFromRule(ComplianceRule rule, Asset asset, string finding)
    {
        var alert = new ComplianceAlert
        {
            AssetId = asset.Id,
            RuleId = rule.Id,
            AlertType = rule.RuleType,
            Title = $"Compliance Violation: {rule.RuleName}",
            Description = finding,
            Severity = rule.Severity,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
            RequiresAction = rule.Severity >= 2,
            DueDate = rule.Severity >= 3 ? DateTime.UtcNow.AddDays(7) : null
        };

        await _complianceRepository.AddAlertAsync(alert);
        return alert;
    }

    private async Task<List<Dictionary<string, object>>> GenerateTrendDataAsync()
    {
        var trendData = new List<Dictionary<string, object>>();
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-30);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayData = new Dictionary<string, object>
            {
                ["date"] = date.ToString("yyyy-MM-dd"),
                ["compliant"] = await _complianceRepository.GetAssetsByStatusCountAsync("Compliant"),
                ["nonCompliant"] = await _complianceRepository.GetAssetsByStatusCountAsync("Non-Compliant"),
                ["warning"] = await _complianceRepository.GetAssetsByStatusCountAsync("Warning")
            };
            trendData.Add(dayData);
        }

        return trendData;
    }

    private async Task<List<TopIssueDto>> GetTopIssuesAsync()
    {
        var alerts = await _complianceRepository.GetActiveAlertsAsync();
        
        return alerts
            .GroupBy(a => a.Title)
            .Select(g => new TopIssueDto
            {
                Issue = g.Key,
                Count = g.Count(),
                Severity = g.Max(a => a.Severity),
                Trend = "stable" // Could calculate based on historical data
            })
            .OrderByDescending(t => t.Severity)
            .ThenByDescending(t => t.Count)
            .Take(5)
            .ToList();
    }

    private ComplianceDashboardDto DeserializeDashboard(ComplianceDashboard dashboard)
    {
        var dto = dashboard.Adapt<ComplianceDashboardDto>();
        
        if (!string.IsNullOrEmpty(dashboard.AssetTypeBreakdown))
        {
            var breakdown = JsonSerializer.Deserialize<Dictionary<string, int>>(dashboard.AssetTypeBreakdown);
            dto.AssetTypeBreakdown = breakdown?.Select(b => new ChartDataDto 
            { 
                Label = b.Key, 
                Value = b.Value 
            }).ToList() ?? new();
        }

        if (!string.IsNullOrEmpty(dashboard.TrendData))
        {
            var trend = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(dashboard.TrendData);
            // Convert trend data as needed
        }

        if (!string.IsNullOrEmpty(dashboard.TopIssues))
        {
            dto.TopIssues = JsonSerializer.Deserialize<List<TopIssueDto>>(dashboard.TopIssues) ?? new();
        }

        return dto;
    }
}