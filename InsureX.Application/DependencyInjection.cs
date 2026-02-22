using InsureX.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InsureX.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register TenantContext as scoped (one per request)
        services.AddScoped<ITenantContext, TenantContext>();
        
        return services;
    }
}