using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using InsureX.Infrastructure.Data;
using InsureX.Infrastructure.Tenant;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Repositories;
using InsureX.Application.Interfaces;
using InsureX.Application.Services;
using InsureX.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System;
using Mapster; // Add this for Mapster
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// ======= Add Configuration =======
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// ======= Add DbContext =======
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.MigrationsAssembly("InsureX.Infrastructure"))
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .LogTo(Console.WriteLine, LogLevel.Information));

// ======= Add Identity =======
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ======= Configure Cookie Authentication =======
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "InsureX.Auth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ======= Add HttpContextAccessor =======
builder.Services.AddHttpContextAccessor();

// ======= Add CORS =======
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ======= Add Response Caching =======
builder.Services.AddResponseCaching();

// ======= Add Health Checks =======
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// ======= Add Razor Pages =======
builder.Services.AddRazorPages();

// ======= Add Controllers with Views =======
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// ======= Add API Explorer =======
builder.Services.AddEndpointsApiExplorer();

// ======= Add Swagger =======
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InsureX API",
        Version = "v1",
        Description = "Insurance Asset Protection & Compliance Platform API"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
});

// ======= Repositories =======
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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

// ======= Background Services =======
builder.Services.AddHostedService<ComplianceBackgroundService>();

// ======= Data Seeder =======
builder.Services.AddScoped<IDataSeeder, DataSeeder>();

// ======= Mapster Configuration =======
TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

// ======= Logging =======
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();
});
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var app = builder.Build();

// ======= Middleware Pipeline =======

// Development middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InsureX API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

// Routing
app.UseRouting();

// CORS
app.UseCors("AllowAll");

// Response caching
app.UseResponseCaching();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Map endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers();

// ======= Ensure Database is Created and Seeded =======
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Apply migrations
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        
        // Seed data
        logger.LogInformation("Seeding initial data...");
        var seeder = services.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync();
        
        logger.LogInformation("Database setup completed successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
    }
}

app.Run();

// For testing purposes
public partial class Program { }