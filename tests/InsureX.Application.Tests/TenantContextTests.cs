using InsureX.Application.Common.Interfaces;
using Xunit;

namespace InsureX.Application.Tests;

public class TenantContextTests
{
    [Fact]
    public void SetTenantId_ValidId_SetsProperty()
    {
        // Arrange
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();

        // Act
        context.SetTenantId(tenantId);

        // Assert
        Assert.True(context.HasTenant);
        Assert.Equal(tenantId, context.TenantId);
        Assert.Equal(tenantId, context.TenantIdOrDefault);
    }

    [Fact]
    public void SetTenantId_EmptyId_ThrowsException()
    {
        // Arrange
        var context = new TenantContext();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.SetTenantId(Guid.Empty));
    }

    [Fact]
    public void TenantId_NotSet_ThrowsException()
    {
        // Arrange
        var context = new TenantContext();

        // Act & Assert
        Assert.False(context.HasTenant);
        Assert.Null(context.TenantIdOrDefault);
        Assert.Throws<InvalidOperationException>(() => context.TenantId);
    }

    [Fact]
    public void Clear_RemovesTenantContext()
    {
        // Arrange
        var context = new TenantContext();
        context.SetTenantId(Guid.NewGuid());

        // Act
        context.Clear();

        // Assert
        Assert.False(context.HasTenant);
        Assert.Null(context.TenantIdOrDefault);
    }
}