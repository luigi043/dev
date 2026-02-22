using System.Security.Claims;
using InsureX.Domain.Interfaces;

namespace InsureX.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Try to set tenant ID from various sources
        Guid? tenantId = null;

        // 1. From authenticated user claims
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id") ?? 
                             context.User.FindFirst("TenantId");
            
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tid))
                tenantId = tid;
        }

        // 2. From headers (for API key authentication)
        if (tenantId == null && context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
        {
            if (Guid.TryParse(headerValue, out var tid))
                tenantId = tid;
        }

        // 3. From subdomain (optional)
        if (tenantId == null)
        {
            var host = context.Request.Host.Host;
            var subdomain = host.Split('.')[0];
            // You might want to lookup tenant by subdomain here
            // tenantId = await _tenantService.GetIdBySubdomain(subdomain);
        }

        if (tenantId.HasValue)
        {
            tenantContext.SetTenantId(tenantId.Value);
        }

        await _next(context);
    }
}

// Extension method for easier registration
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}