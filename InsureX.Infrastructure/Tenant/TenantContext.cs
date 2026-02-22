using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InsureX.Infrastructure.Tenant;  // This is a namespace, not the entity

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
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = httpContext.User.FindFirst("TenantId");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                return tenantId;
            }
        }
        return _tenantId.Value;
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public static void SetTenantId(Guid? tenantId)
    {
        _tenantId.Value = tenantId;
    }
}