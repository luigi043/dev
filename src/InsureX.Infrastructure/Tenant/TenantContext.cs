using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InsureX.Infrastructure.Tenant;

public interface ITenantContext
{
    Guid? GetCurrentTenantId();
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
}

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly AsyncLocal<Guid?> _tenantId = new();

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentTenantId()
    {
        // Try to get from HttpContext first
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = httpContext.User.FindFirst("TenantId");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                return tenantId;
            }
        }

        // Try to get from header
        if (httpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) == true)
        {
            if (Guid.TryParse(tenantHeader, out var tenantId))
            {
                return tenantId;
            }
        }

        // Fallback to AsyncLocal for background services
        return _tenantId.Value;
    }

    public string? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext?.User?.FindFirst("sub")?.Value;
    }

    public string? GetCurrentUserEmail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    // For background services
    public static void SetTenantId(Guid? tenantId)
    {
        _tenantId.Value = tenantId;
    }
}