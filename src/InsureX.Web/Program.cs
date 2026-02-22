using InsureX.Application.Interfaces;
using InsureX.Application.Services;
using InsureX.Application.Services.Helpers;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



var builder = WebApplication.CreateBuilder(args);

// ======= Add Razor Pages =======
builder.Services.AddRazorPages();

// ======= Repositories =======
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IComplianceRepository, ComplianceRepository>();

// ======= Services =======
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IComplianceEngineService, ComplianceEngineService>();


// ======= Contexts / Helpers =======
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


// ======= Logging =======
builder.Services.AddLogging();

// ======= Controllers / MVC =======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ======= Middleware =======
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======= Configure the HTTP request pipeline =======
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();namespace InsureX.Application.Services { public class Program { } }