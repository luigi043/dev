using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Mapster;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// ======= Project Imports =======
using InsureX.Infrastructure.Data;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Repositories;
using InsureX.Application.Interfaces;
using InsureX.Application.Services;
using InsureX.Infrastructure.Services;
using InsureX.Infrastructure.BackgroundServices;
using InsureX.Api.Middleware; // For tenant middleware

var builder = WebApplication.CreateBuilder(args);

// ======= 1. Configuration Setup =======
ConfigureConfiguration(builder);

// ======= 2. Service Registration =======
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// ======= 3. Middleware Pipeline =======
ConfigureMiddleware(app);

// ======= 4. Database Initialization =======
await InitializeDatabaseAsync(app);

// ======= 5. Run Application =======
await RunApplicationAsync(app);

// ============================================================================
// CONFIGURATION METHODS
// ============================================================================

void ConfigureConfiguration(WebApplicationBuilder builder)
{
    // Add configuration sources
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
    builder.Configuration.AddEnvironmentVariables();
    
    // Add user secrets in development
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    // ======= Database Context =======
    ConfigureDatabase(services, configuration, environment);
    
    // ======= Identity & Authentication =======
    ConfigureIdentity(services);
    
    // ======= Core ASP.NET Services =======
    ConfigureCoreServices(services);
    
    // ======= API & Documentation =======
    ConfigureApiServices(services, environment);
    
    // ======= Dependency Injection =======
    ConfigureDependencyInjection(services);
    
    // ======= Mapster Configuration =======
    ConfigureMapster();
    
    // ======= Logging =======
    ConfigureLogging(services);
    
    // ======= Health Checks =======
    ConfigureHealthChecks(services);
    
    // ======= CORS =======
    ConfigureCors(services, environment);
    
    // ======= EPPlus License =======
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
}

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");
    }
    
    services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        options.UseSqlServer(
            connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.MigrationsAssembly("InsureX.Infrastructure");
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        
        if (environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
    });
}

void ConfigureIdentity(IServiceCollection services)
{
    // FIXED: Use int for Identity (IdentityUser<int>)
    services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
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

    // Configure cookie authentication
    services.ConfigureApplicationCookie(options =>
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

void ConfigureCoreServices(IServiceCollection services)
{
    services.AddHttpContextAccessor();
    services.AddResponseCaching();
    services.AddMemoryCache();
    
    // Add distributed cache (use SQL Server or Redis in production)
    services.AddDistributedMemoryCache();
    
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "InsureX.Session";
    });
    
    services.AddRazorPages();
    services.AddControllersWithViews()
        .AddRazorRuntimeCompilation()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.WriteIndented = true;
        });
    
    services.AddEndpointsApiExplorer();
}

void ConfigureApiServices(IServiceCollection services, IHostEnvironment environment)
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "InsureX API",
            Version = "v1",
            Description = "Insurance Asset Protection & Compliance Platform API",
            Contact = new OpenApiContact
            {
                Name = "InsureX Team",
                Email = "support@insurex.com",
                Url = new Uri("https://insurex.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });
        
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
        });
        
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        
        // Include XML comments
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });
}

void ConfigureDependencyInjection(IServiceCollection services)
{
    // ===== TENANT CONTEXT (CRITICAL) =====
    services.AddScoped<ITenantContext, TenantContext>();
    
    // Repositories
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddScoped<IPolicyRepository, PolicyRepository>();
    services.AddScoped<IAssetRepository, AssetRepository>();
    services.AddScoped<IAuditRepository, AuditRepository>();
    services.AddScoped<IComplianceRepository, ComplianceRepository>();

    // Services
    services.AddScoped<IPolicyService, PolicyService>();
    services.AddScoped<IAssetService, AssetService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IComplianceEngineService, ComplianceEngineService>();
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Data Seeder
    services.AddScoped<IDataSeeder, DataSeeder>();

    // Background Services
    services.AddHostedService<ComplianceBackgroundService>();
    
    // HTTP Clients
    services.AddHttpClient("InsurerApi", client =>
    {
        client.BaseAddress = new Uri("https://api.insurer.com/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}

void ConfigureMapster()
{
    TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
    TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
    
    // Custom mappings
    TypeAdapterConfig<ApplicationUser, UserDto>.NewConfig()
        .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}".Trim());
}

void ConfigureLogging(IServiceCollection services)
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.AddEventSourceLogger();
        logging.AddEventLog();
        
        // Add Application Insights if configured
        // logging.AddApplicationInsights();
    });
}

void ConfigureHealthChecks(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("Database", HealthStatus.Unhealthy)
        .AddUrlGroup(new Uri("https://localhost:5001"), "Web App", HealthStatus.Degraded)
        .AddUrlGroup(new Uri("https://localhost:5002"), "API", HealthStatus.Degraded);
}

void ConfigureCors(IServiceCollection services, IHostEnvironment environment)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecific", policy =>
        {
            policy.WithOrigins(
                    "https://localhost:5001", 
                    "https://localhost:5002",
                    "https://insurex-web.azurewebsites.net",
                    "https://insurex-api.azurewebsites.net")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
        
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}

// ============================================================================
// MIDDLEWARE CONFIGURATION
// ============================================================================

void ConfigureMiddleware(WebApplication app)
{
    var environment = app.Environment;
    
    // Development middleware
    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => 
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "InsureX API V1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "InsureX API Documentation";
        });
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Security middleware
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Routing
    app.UseRouting();

    // CORS - Use specific policy for production
    if (environment.IsDevelopment())
    {
        app.UseCors("AllowAll");
    }
    else
    {
        app.UseCors("AllowSpecific");
    }

    // Session
    app.UseSession();

    // Response caching
    app.UseResponseCaching();

    // Custom middleware - ORDER IS CRITICAL!
    app.UseMiddleware<RequestLoggingMiddleware>();          // 1. Log all requests
    
    // TENANT MIDDLEWARE - Must come AFTER logging but BEFORE authentication
    // FIXED: Using the correct middleware from Api.Middleware namespace
    app.UseMiddleware<TenantResolutionMiddleware>();        // 2. Resolve tenant from request
    
    app.UseMiddleware<ExceptionHandlingMiddleware>();       // 3. Global exception handling

    // Authentication & Authorization - Must come AFTER tenant resolution
    app.UseAuthentication();                                 // 4. Authenticate user
    app.UseAuthorization();                                  // 5. Authorize user

    // Health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = WriteHealthCheckResponse,
        AllowCachingResponses = false,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy] = StatusCodes.Status200OK,
            [HealthStatus.Degraded] = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });

    // Map endpoints
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    app.MapRazorPages();
    app.MapControllers();
}

// ============================================================================
// DATABASE INITIALIZATION
// ============================================================================

async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        var context = services.GetRequiredService<AppDbContext>();
        var seeder = services.GetRequiredService<IDataSeeder>();
        
        // Test database connection
        if (await TestDatabaseConnectionAsync(context, logger))
        {
            // Apply migrations
            await ApplyMigrationsAsync(context, logger);
            
            // Seed data
            await SeedDatabaseAsync(seeder, logger);
            
            logger.LogInformation("Database initialization completed successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization");
        
        if (app.Environment.IsDevelopment())
        {
            throw; // Rethrow in development to see the error
        }
    }
}

async Task<bool> TestDatabaseConnectionAsync(AppDbContext context, ILogger logger)
{
    try
    {
        logger.LogInformation("Testing database connection...");
        var canConnect = await context.Database.CanConnectAsync();
        
        if (canConnect)
        {
            logger.LogInformation("Database connection successful");
            return true;
        }
        else
        {
            logger.LogWarning("Cannot connect to database. Please check connection string.");
            return false;
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database connection test failed");
        return false;
    }
}

async Task ApplyMigrationsAsync(AppDbContext context, ILogger logger)
{
    try
    {
        logger.LogInformation("Applying database migrations...");
        
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingCount = pendingMigrations.Count();
        
        if (pendingCount > 0)
        {
            logger.LogInformation("Found {Count} pending migrations", pendingCount);
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully");
        }
        else
        {
            logger.LogInformation("No pending migrations found");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations");
        throw;
    }
}

async Task SeedDatabaseAsync(IDataSeeder seeder, ILogger logger)
{
    try
    {
        logger.LogInformation("Seeding initial data...");
        await seeder.SeedAsync();
        logger.LogInformation("Data seeding completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding database");
        throw;
    }
}

// ============================================================================
// APPLICATION STARTUP
// ============================================================================

async Task RunApplicationAsync(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting InsureX application...");
        logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
        logger.LogInformation("URLs: https://localhost:5001, https://localhost:5002");
        
        await app.RunAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Application failed to start");
        throw;
    }
}

// ============================================================================
// HELPER METHODS
// ============================================================================

async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    
    var response = new
    {
        status = report.Status.ToString(),
        duration = report.TotalDuration.ToString(),
        timestamp = DateTime.UtcNow,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.ToString(),
            tags = e.Value.Tags,
            data = e.Value.Data
        })
    };
    
    await context.Response.WriteAsJsonAsync(response);
}