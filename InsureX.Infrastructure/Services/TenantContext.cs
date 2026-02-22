using InsureX.Application.Interfaces;

namespace InsureX.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    
    public Guid? TenantId => _tenantId;
    
    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
    
    public bool HasValidTenant => _tenantId.HasValue && _tenantId != Guid.Empty;
}

// Interface in Application layer
namespace InsureX.Application.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    void SetTenantId(Guid tenantId);
    bool HasValidTenant { get; }
}