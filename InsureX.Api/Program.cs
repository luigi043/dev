using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Mapster;
using OfficeOpenXml;
using System;
using System.Threading.Tasks;

// ======= Namespace Imports =======
using InsureX.Infrastructure.Data;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Repositories;
using InsureX.Application.Interfaces;
using InsureX.Application.Services;
using InsureX.Infrastructure.Services;
using InsureX.Infrastructure.Tenant;

namespace InsureX.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ======= 1. CONFIGURATION =======
        ConfigureConfiguration(builder);

        // ======= 2. DATABASE & IDENTITY =======
        ConfigureDatabase(builder);
        ConfigureIdentity(builder);
        ConfigureAuthentication(builder);

        // ======= 3. CORE SERVICES =======
        ConfigureCoreServices(builder);

        // ======= 4. APPLICATION SERVICES =======
        ConfigureApplicationServices(builder);

        // ======= 5. REPOSITORIES =======
        ConfigureRepositories(builder);

        // ======= 6. API & SWAGGER =======
        ConfigureApi(builder);

        // ======= 7. BACKGROUND SERVICES =======
        ConfigureBackgroundServices(builder);

        // ======= 8. MAPSTER CONFIGURATION =======
        ConfigureMapster();

        // ======= 9. EPPLUS CONFIGURATION =======
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var app = builder.Build();

        // ======= 10. MIDDLEWARE PIPELINE =======
        ConfigureMiddleware(app);

        // ======= 11. DATABASE MIGRATION & SEEDING =======
        await MigrateAndSeedDatabase(app);

        app.Run();
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions => sqlServerOptions.MigrationsAssembly("InsureX.Infrastructure"))
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .LogTo(Console.WriteLine, LogLevel.Information));
    }

    private static void ConfigureIdentity(WebApplicationBuilder builder)
    {
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
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
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
    }

    private static void ConfigureCoreServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        builder.Services.AddResponseCaching();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();
        });
    }

    private static void ConfigureApplicationServices(WebApplicationBuilder builder)
    {
        // Contexts / Helpers
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Services
        builder.Services.AddScoped<IPolicyService, PolicyService>();
        builder.Services.AddScoped<IAssetService, AssetService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IComplianceEngineService, ComplianceEngineService>();

        // Data Seeder
        builder.Services.AddScoped<IDataSeeder, DataSeeder>();
    }

    private static void ConfigureRepositories(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
        builder.Services.AddScoped<IAssetRepository, AssetRepository>();
        builder.Services.AddScoped<IAuditRepository, AuditRepository>();
        builder.Services.AddScoped<IComplianceRepository, ComplianceRepository>();
    }

    private static void ConfigureApi(WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews()
            .AddRazorRuntimeCompilation();
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();

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
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
            });
        });
    }

    private static void ConfigureBackgroundServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<ComplianceBackgroundService>();
    }

    private static void ConfigureMapster()
    {
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        
        // Add custom mappings here if needed
        // TypeAdapterConfig<Source, Destination>.NewConfig()
        //     .Map(dest => dest.Property, src => src.OtherProperty);
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
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
    }

    private static async Task MigrateAndSeedDatabase(WebApplication app)
    {
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
    }
}

// For testing purposes
public partial class Program { }