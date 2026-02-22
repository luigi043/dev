using InsureX.Application.Interfaces;

namespace InsureX.Application.Services.Helpers
{
    public class TenantContext : ITenantContext
    {
        public int? GetCurrentTenantId() => 1; // Temporary placeholder
    }
}