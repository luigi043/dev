using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using InsureX.Domain.Interfaces;
using System.Security.Claims;

namespace InsureX.Infrastructure.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        try
        {
            // Method 1: From authenticated user claims
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst("tenant_id") ?? 
                                    context.User.FindFirst("TenantId") ??
                                    context.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/tenantid");

                // FIXED: Parse as int, not Guid
                if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    tenantContext.SetTenantId(tenantId);
                    _logger.LogDebug("Tenant context set from claims: {TenantId}", tenantId);
                    
                    // Add to response headers for debugging
                    context.Response.Headers.Add("X-Tenant-Id", tenantId.ToString());
                }
            }

            // Method 2: From header (for API keys, background jobs)
            if (!tenantContext.HasTenant && context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
            {
                // FIXED: Parse as int
                if (int.TryParse(tenantIdHeader, out var tenantId))
                {
                    tenantContext.SetTenantId(tenantId);
                    _logger.LogDebug("Tenant context set from header: {TenantId}", tenantId);
                }
            }

            // Method 3: From subdomain (e.g., tenant1.insurex.com)
            if (!tenantContext.HasTenant && context.Request.Host.Host.Contains('.'))
            {
                var subdomain = context.Request.Host.Host.Split('.')[0];
                // You might want to resolve subdomain to tenant ID via a cache or service
                _logger.LogDebug("Potential tenant from subdomain: {Subdomain}", subdomain);
            }

            await _next(context);
        }
        finally
        {
            // Always clear tenant context after request to prevent leakage
            tenantContext.Clear();
        }
    }
}