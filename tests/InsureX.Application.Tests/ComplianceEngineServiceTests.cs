using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using InsureX.Application.Services;
using InsureX.Application.Interfaces;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Entities;
using InsureX.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace InsureX.Application.Tests;

public class ComplianceEngineServiceTests
{
    [Fact]
    public async Task CheckAssetComplianceAsync_Returns_NonCompliant_When_InspectionOverdue()
    {
        var complianceRepo = new Mock<IComplianceRepository>();
        var assetRepo = new Mock<IAssetRepository>();
        var tenantCtx = new Mock<ITenantContext>();
        var notification = new Mock<INotificationService>();
        var logger = new Mock<ILogger<ComplianceEngineService>>();

        var asset = new Asset { Id = 1, AssetTag = "A1", LastInspectionDate = DateTime.UtcNow.AddYears(-1), CreatedAt = DateTime.UtcNow.AddYears(-2) };
        assetRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);

        var rule = new ComplianceRule { Id = 1, RuleCode = "INS001", RuleType = "Inspection", DaysToExpiry = 30, Severity = 3, IsActive = true };
        complianceRepo.Setup(r => r.GetActiveRulesAsync()).ReturnsAsync(new List<ComplianceRule> { rule });
        complianceRepo.Setup(r => r.AddAsync(It.IsAny<ComplianceCheck>())).Returns(Task.CompletedTask);
        complianceRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var svc = new ComplianceEngineService(complianceRepo.Object, assetRepo.Object, tenantCtx.Object, notification.Object, logger.Object);

        var result = await svc.CheckAssetComplianceAsync(1);

        Assert.Equal(1, result.AssetId);
        Assert.True(result.Score < 80);
        Assert.Contains("Inspection overdue", result.Findings);
    }
}
