using InsureX.Api.Middleware;
using InsureX.Application.Common.Interfaces;

namespace InsureX.Api.Extensions;

public static class TenantServiceExtensions
{
    /// <summary>
    /// Adds tenant resolution services to the DI container
    /// </summary>
    public static IServiceCollection AddTenantResolution(this IServiceCollection services, 
        Action<TenantMiddlewareOptions>? configureOptions = null)
    {
        // Configure options
        var options = new TenantMiddlewareOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(Options.Create(options));

        // Register tenant context
        services.AddScoped<ITenantContext, TenantContext>();

        return services;
    }

    /// <summary>
    /// Configures the tenant resolution middleware with specific options
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app, 
        Action<TenantMiddlewareOptions>? configureOptions = null)
    {
        // Apply configuration if provided
        if (configureOptions != null)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<TenantMiddlewareOptions>>().Value;
            configureOptions(options);
        }

        return app.UseMiddleware<TenantMiddleware>();
    }
}