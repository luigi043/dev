using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InsureX.Infrastructure.Services;

public class ComplianceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ComplianceBackgroundService> _logger;

    public ComplianceBackgroundService(
        IServiceProvider services,
        ILogger<ComplianceBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Compliance Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformComplianceChecks();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in compliance background service");
            }

            // Run every 6 hours
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task PerformComplianceChecks()
    {
        using var scope = _services.CreateScope();
        var complianceService = scope.ServiceProvider
            .GetRequiredService<IComplianceEngineService>();

        _logger.LogInformation("Starting automated compliance checks");

        try
        {
            // Check assets needing update
            var results = await complianceService.CheckAssetsNeedingUpdateAsync();
            _logger.LogInformation("Checked {Count} assets needing update", results.Count);

            // Update expired rules
            var expiredRules = await complianceService.UpdateExpiredRulesAsync();
            _logger.LogInformation("Updated {Count} expired rules", expiredRules);

            // Resolve stale alerts
            var staleAlerts = await complianceService.ResolveStaleAlertsAsync(30);
            _logger.LogInformation("Resolved {Count} stale alerts", staleAlerts);

            _logger.LogInformation(
                "Compliance check completed: {Results} assets checked, {ExpiredRules} rules expired, {StaleAlerts} alerts resolved",
                results.Count, expiredRules, staleAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during compliance checks");
        }
    }
}
