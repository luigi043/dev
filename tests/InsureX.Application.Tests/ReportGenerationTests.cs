using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using InsureX.Application.Services;
using InsureX.Application.Interfaces;
using InsureX.Domain.Interfaces;
using InsureX.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsureX.Application.Tests;

public class ReportGenerationTests
{
    [Fact]
    public async Task GenerateComplianceReportAsync_Returns_Excel_Bytes()
    {
        var complianceRepo = new Mock<IComplianceRepository>();
        var assetRepo = new Mock<IAssetRepository>();
        var tenantCtx = new Mock<ITenantContext>();
        var notification = new Mock<INotificationService>();
        var logger = new Mock<ILogger<ComplianceEngineService>>();

        // Provide a sample check
        var check = new ComplianceCheck { AssetId = 1, CheckDate = DateTime.UtcNow, Score = 50, Status = "Warning", Findings = "Test" };
        complianceRepo.Setup(r => r.GetCheckHistoryAsync(0, 365)).ReturnsAsync(new System.Collections.Generic.List<ComplianceCheck> { check });
        assetRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Asset { Id = 1, AssetTag = "A1" });

        var svc = new ComplianceEngineService(complianceRepo.Object, assetRepo.Object, tenantCtx.Object, notification.Object, logger.Object);

        var bytes = await svc.GenerateComplianceReportAsync();

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }
}
