using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using InsureX.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsureX.Api.Middleware;

/// <summary>
/// Middleware that resolves and sets the current tenant context from various sources
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        try
        {
            // Try to set tenant ID from various sources
            Guid? tenantId = null;
            string? source = null;

            // 1. From authenticated user claims (highest priority)
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                tenantId = ExtractTenantFromClaims(context.User);
                if (tenantId.HasValue)
                    source = "user claims";
            }

            // 2. From headers (for API key authentication / system integrations)
            if (!tenantId.HasValue && 
                context.Request.Headers.TryGetValue("X-Tenant-Id", out StringValues headerValue))
            {
                tenantId = ParseTenantId(headerValue.ToString());
                if (tenantId.HasValue)
                    source = "X-Tenant-Id header";
            }

            // 3. From subdomain (for multi-tenant SaaS portals)
            if (!tenantId.HasValue)
            {
                tenantId = ExtractTenantFromSubdomain(context.Request.Host.Host);
                if (tenantId.HasValue)
                    source = "subdomain";
            }

            // 4. From query string (for development/testing - NEVER in production!)
            #if DEBUG
            if (!tenantId.HasValue && 
                context.Request.Query.TryGetValue("tenantId", out StringValues queryValue))
            {
                tenantId = ParseTenantId(queryValue.ToString());
                if (tenantId.HasValue)
                    source = "query string (DEBUG ONLY)";
            }
            #endif

            // Set the tenant context if found
            if (tenantId.HasValue)
            {
                tenantContext.SetTenantId(tenantId.Value);
                _logger.LogDebug("Tenant context set to {TenantId} from {Source}", 
                    tenantId.Value, source);
                
                // Add tenant ID to response headers for debugging/tracking
                context.Response.Headers.TryAdd("X-Tenant-Id", tenantId.Value.ToString());
            }
            else
            {
                // For public endpoints, this might be acceptable
                // For protected endpoints, authentication will handle it
                _logger.LogDebug("No tenant context could be resolved for request: {Path}", 
                    context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - let the request continue
            // The TenantContext will throw when accessed if needed
            _logger.LogError(ex, "Error resolving tenant context for request: {Path}", 
                context.Request.Path);
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts tenant ID from user claims
    /// </summary>
    private Guid? ExtractTenantFromClaims(ClaimsPrincipal user)
    {
        // Try different claim types commonly used for tenant ID
        var tenantClaim = user.FindFirst("tenant_id") ?? 
                         user.FindFirst("TenantId") ??
                         user.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid") ??
                         user.FindFirst("tenant");

        if (tenantClaim != null)
            return ParseTenantId(tenantClaim.Value);

        return null;
    }

    /// <summary>
    /// Extracts tenant ID from subdomain (assumes tenant ID is the subdomain)
    /// </summary>
    private Guid? ExtractTenantFromSubdomain(string host)
    {
        if (string.IsNullOrEmpty(host))
            return null;

        var parts = host.Split('.');
        if (parts.Length < 2)
            return null;

        var subdomain = parts[0];
        
        // Skip common subdomains that aren't tenant IDs
        if (subdomain.Equals("www", StringComparison.OrdinalIgnoreCase) ||
            subdomain.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            subdomain.Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Try to parse as GUID
        if (Guid.TryParse(subdomain, out var guidTenantId))
            return guidTenantId;

        // If not a GUID, you might want to lookup by subdomain from a cache/db
        // This would require injecting a service
        // var tenantId = await _tenantService.GetIdBySubdomain(subdomain);
        
        return null;
    }

    /// <summary>
    /// Safely parses a tenant ID from string
    /// </summary>
    private Guid? ParseTenantId(string? tenantIdString)
    {
        if (string.IsNullOrWhiteSpace(tenantIdString))
            return null;

        if (Guid.TryParse(tenantIdString, out var guid))
            return guid;

        _logger.LogWarning("Failed to parse tenant ID from string: {TenantIdString}", 
            tenantIdString);
        return null;
    }
}

/// <summary>
/// Extension methods for registering the tenant middleware
/// </summary>
public static class TenantMiddlewareExtensions
{
    /// <summary>
    /// Adds the tenant resolution middleware to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}

/// <summary>
/// Options for configuring tenant resolution behavior
/// </summary>
public class TenantMiddlewareOptions
{
    /// <summary>
    /// Whether to allow tenant resolution from query string (development only)
    /// </summary>
    public bool AllowQueryStringTenant { get; set; } = false;

    /// <summary>
    /// Header name for tenant ID (default: X-Tenant-Id)
    /// </summary>
    public string TenantIdHeaderName { get; set; } = "X-Tenant-Id";

    /// <summary>
    /// Whether to add tenant ID to response headers
    /// </summary>
    public bool AddTenantToResponseHeaders { get; set; } = true;

    /// <summary>
    /// Default tenant ID to use when none is resolved (development only)
    /// </summary>
    public Guid? DefaultTenantId { get; set; }
}

/// <summary>
/// Enhanced middleware with options pattern support
/// </summary>
public class ConfigurableTenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConfigurableTenantMiddleware> _logger;
    private readonly TenantMiddlewareOptions _options;

    public ConfigurableTenantMiddleware(
        RequestDelegate next,
        ILogger<ConfigurableTenantMiddleware> logger,
        IOptions<TenantMiddlewareOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Your tenant resolution logic here, using _options
        // ... (similar to above but with configurable behavior)

        await _next(context);
    }
}