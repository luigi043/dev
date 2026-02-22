using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using InsureX.Domain.Interfaces;

namespace InsureX.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private int? _tenantId;

    public TenantContext(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Properties
    public int? TenantId => _tenantId;
    public bool HasTenant => _tenantId.HasValue;

    // Methods
    public void SetTenantId(int tenantId)
    {
        if (tenantId <= 0) 
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));
        _tenantId = tenantId;
    }

    public int? GetCurrentTenantId() => _tenantId;

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor?.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor?.HttpContext?.User?
            .FindFirstValue(ClaimTypes.Email);
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor?.HttpContext?.User?
            .Identity?.IsAuthenticated ?? false;
    }

    public string[] GetUserRoles()
    {
        return _httpContextAccessor?.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? Array.Empty<string>();
    }

    public DateTime GetCurrentRequestTime() => DateTime.UtcNow;

    public string? GetCurrentUserFullName()
    {
        var ctx = _httpContextAccessor?.HttpContext;
        if (ctx == null) return null;
        
        var given = ctx.User?.FindFirst("given_name")?.Value;
        var family = ctx.User?.FindFirst("family_name")?.Value;
        
        if (!string.IsNullOrEmpty(given) || !string.IsNullOrEmpty(family))
            return string.Join(' ', new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));
            
        return ctx.User?.Identity?.Name;
    }

    public string? GetClientIpAddress()
    {
        try
        {
            return _httpContextAccessor?.HttpContext?
                .Connection?.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        _tenantId = null;
    }
}