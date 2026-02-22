using InsureX.Application.Interfaces;

namespace InsureX.Application.Services.Helpers
{
    public class TenantContext : ITenantContext
    {
        public System.Guid? GetCurrentTenantId() => System.Guid.NewGuid(); // Temporary placeholder
    }
}