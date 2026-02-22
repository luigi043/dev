using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using InsureX.Application.Interfaces;
using InsureX.Application.Services;
using FluentValidation;
using MediatR;

namespace InsureX.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register MediatR for CQRS patterns
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            
            // Register Application Services
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IComplianceService, ComplianceService>();
            services.AddScoped<IComplianceEngineService, ComplianceEngineService>();
            services.AddScoped<IScriptRuleEvaluator, ScriptRuleEvaluator>();
            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, NotificationService>();
            
            // Note: Repositories and TenantContext are registered in Infrastructure layer
            
            return services;
        }
    }
}