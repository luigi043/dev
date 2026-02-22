using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using InsureX.Domain.Interfaces;

namespace InsureX.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private readonly AsyncLocal<Guid?> _tenantId = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public TenantContext(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId => _tenantId.Value;

    public void SetTenantId(Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        _tenantId.Value = tenantId;
    }

    public Guid? GetCurrentTenantId() => _tenantId.Value;

    public string? GetCurrentUserId() => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? GetCurrentUserEmail() => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated() => _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string[] GetUserRoles() => _httpContextAccessor?.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

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
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
