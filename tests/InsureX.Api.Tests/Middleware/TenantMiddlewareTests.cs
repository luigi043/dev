using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using InsureX.Api.Middleware;
using InsureX.Application.Common.Interfaces;
using System.Security.Claims;

namespace InsureX.Api.Tests.Middleware;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithValidClaim_SetsTenantContext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tenant_id", tenantId.ToString())
        }, "test"));

        var tenantContext = new Mock<ITenantContext>();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantMiddleware(next, NullLogger<TenantMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, tenantContext.Object);

        // Assert
        tenantContext.Verify(x => x.SetTenantId(tenantId), Times.Once);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithHeader_SetsTenantContext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("X-Tenant-Id", tenantId.ToString());

        var tenantContext = new Mock<ITenantContext>();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantMiddleware(next, NullLogger<TenantMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, tenantContext.Object);

        // Assert
        tenantContext.Verify(x => x.SetTenantId(tenantId), Times.Once);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NoTenant_DoesNotSetContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var tenantContext = new Mock<ITenantContext>();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantMiddleware(next, NullLogger<TenantMiddleware>.Instance);

        // Act
        await middleware.InvokeAsync(context, tenantContext.Object);

        // Assert
        tenantContext.Verify(x => x.SetTenantId(It.IsAny<Guid>()), Times.Never);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_InvalidGuid_LogsWarning()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("X-Tenant-Id", "invalid-guid");

        var tenantContext = new Mock<ITenantContext>();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var logger = new Mock<ILogger<TenantMiddleware>>();
        var middleware = new TenantMiddleware(next, logger.Object);

        // Act
        await middleware.InvokeAsync(context, tenantContext.Object);

        // Assert
        tenantContext.Verify(x => x.SetTenantId(It.IsAny<Guid>()), Times.Never);
        Assert.True(nextCalled);
        // Verify warning was logged (implementation depends on your logging setup)
    }
}