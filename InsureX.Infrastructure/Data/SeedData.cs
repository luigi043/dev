using InsureX.Domain.Entities;
using InsureX.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsureX.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            logger.LogInformation("Starting database seeding...");

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Create roles if they don't exist
            await SeedRolesAsync(roleManager, logger);

            // Create tenants
            var tenants = await SeedTenantsAsync(context, logger);

            // Create users for each tenant
            await SeedUsersAsync(userManager, tenants, logger);

            // Seed sample assets for each tenant
            await SeedAssetsAsync(context, tenants, logger);

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = { "Admin", "Financer", "Insurer", "Broker", "Auditor" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                logger.LogInformation("Creating role: {Role}", role);
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create role {Role}: {Errors}", 
                        role, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task<List<Tenant>> SeedTenantsAsync(AppDbContext context, ILogger logger)
    {
        var tenants = new List<Tenant>();

        // Default tenant with fixed GUID for consistency
        var defaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var defaultTenant = await context.Tenants.FindAsync(defaultTenantId);
        
        if (defaultTenant == null)
        {
            logger.LogInformation("Creating default tenant...");
            defaultTenant = new Tenant
            {
                Id = defaultTenantId,
                Name = "Default Bank",
                Subdomain = "default",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ContactEmail = "admin@defaultbank.com",
                ContactPhone = "+1-555-0123",
                Address = "123 Main Street, New York, NY 10001",
                Settings = new TenantSettings
                {
                    PrimaryColor = "#007bff",
                    SecondaryColor = "#6c757d",
                    LogoUrl = "/images/default-logo.png",
                    DateFormat = "MM/dd/yyyy",
                    TimeZone = "Eastern Standard Time"
                }
            };
            await context.Tenants.AddAsync(defaultTenant);
            tenants.Add(defaultTenant);
        }
        else
        {
            tenants.Add(defaultTenant);
        }

        // Create additional sample tenants
        var sampleTenants = new[]
        {
            new { Name = "First National Bank", Subdomain = "fnb", Id = Guid.Parse("22222222-2222-2222-2222-222222222222") },
            new { Name = "Liberty Insurance", Subdomain = "liberty", Id = Guid.Parse("33333333-3333-3333-3333-333333333333") },
            new { Name = "Coastal Finance", Subdomain = "coastal", Id = Guid.Parse("44444444-4444-4444-4444-444444444444") }
        };

        foreach (var sample in sampleTenants)
        {
            var existingTenant = await context.Tenants.FindAsync(sample.Id);
            if (existingTenant == null)
            {
                logger.LogInformation("Creating sample tenant: {TenantName}", sample.Name);
                var tenant = new Tenant
                {
                    Id = sample.Id,
                    Name = sample.Name,
                    Subdomain = sample.Subdomain,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ContactEmail = $"admin@{sample.Subdomain}.com",
                    ContactPhone = $"+1-555-{new Random().Next(1000, 9999)}",
                    Address = $"456 Business Ave, Suite {new Random().Next(100, 999)}, Chicago, IL 60601",
                    Settings = new TenantSettings
                    {
                        PrimaryColor = GetRandomColor(),
                        SecondaryColor = GetRandomColor(),
                        LogoUrl = $"/images/{sample.Subdomain}-logo.png",
                        DateFormat = "MM/dd/yyyy",
                        TimeZone = "Central Standard Time"
                    }
                };
                await context.Tenants.AddAsync(tenant);
                tenants.Add(tenant);
            }
            else
            {
                tenants.Add(existingTenant);
            }
        }

        await context.SaveChangesAsync();
        return tenants;
    }

    private static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager, 
        List<Tenant> tenants, 
        ILogger logger)
    {
        var userConfigs = new[]
        {
            // Admin users
            new { Email = "admin@insurex.com", Role = "Admin", FirstName = "System", LastName = "Admin", TenantIndex = 0 },
            new { Email = "admin@fnb.com", Role = "Admin", FirstName = "Sarah", LastName = "Johnson", TenantIndex = 1 },
            new { Email = "admin@liberty.com", Role = "Admin", FirstName = "Michael", LastName = "Chen", TenantIndex = 2 },
            
            // Financer users
            new { Email = "financer@insurex.com", Role = "Financer", FirstName = "James", LastName = "Wilson", TenantIndex = 0 },
            new { Email = "financer@fnb.com", Role = "Financer", FirstName = "Maria", LastName = "Garcia", TenantIndex = 1 },
            new { Email = "financer@coastal.com", Role = "Financer", FirstName = "Robert", LastName = "Taylor", TenantIndex = 3 },
            
            // Insurer users
            new { Email = "insurer@insurex.com", Role = "Insurer", FirstName = "Patricia", LastName = "Brown", TenantIndex = 0 },
            new { Email = "insurer@liberty.com", Role = "Insurer", FirstName = "David", LastName = "Martinez", TenantIndex = 2 },
            new { Email = "insurer@coastal.com", Role = "Insurer", FirstName = "Lisa", LastName = "Anderson", TenantIndex = 3 },
            
            // Broker users
            new { Email = "broker@insurex.com", Role = "Broker", FirstName = "Thomas", LastName = "Moore", TenantIndex = 0 },
            new { Email = "broker@fnb.com", Role = "Broker", FirstName = "Jennifer", LastName = "Lee", TenantIndex = 1 },
            
            // Auditor users
            new { Email = "auditor@insurex.com", Role = "Auditor", FirstName = "William", LastName = "Davis", TenantIndex = 0 }
        };

        foreach (var config in userConfigs)
        {
            var tenant = tenants[config.TenantIndex < tenants.Count ? config.TenantIndex : 0];
            
            if (await userManager.FindByEmailAsync(config.Email) == null)
            {
                logger.LogInformation("Creating user: {Email} with role: {Role} for tenant: {Tenant}", 
                    config.Email, config.Role, tenant.Name);

                var user = new ApplicationUser
                {
                    UserName = config.Email,
                    Email = config.Email,
                    FirstName = config.FirstName,
                    LastName = config.LastName,
                    TenantId = tenant.Id,
                    EmailConfirmed = true,
                    PhoneNumber = $"+1-555-{new Random().Next(100, 999)}-{new Random().Next(1000, 9999)}",
                    PhoneNumberConfirmed = true,
                    LastLoginAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                    IsActive = true
                };

                // Generate password based on role
                string password = GeneratePassword(config.Role);
                
                var result = await userManager.CreateAsync(user, password);
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, config.Role);
                    logger.LogInformation("Successfully created user: {Email}", config.Email);
                }
                else
                {
                    logger.LogError("Failed to create user {Email}: {Errors}", 
                        config.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedAssetsAsync(AppDbContext context, List<Tenant> tenants, ILogger logger)
    {
        if (!await context.Assets.AnyAsync())
        {
            logger.LogInformation("Seeding sample assets...");

            var random = new Random();
            var makes = new[] { "Toyota", "Honda", "Ford", "Chevrolet", "BMW", "Mercedes", "Audi", "Lexus" };
            var models = new[] { "Camry", "Civic", "F-150", "Silverado", "X5", "C-Class", "A4", "RX" };
            var statuses = Enum.GetValues<AssetStatus>();

            foreach (var tenant in tenants)
            {
                var assetCount = random.Next(5, 15);
                var assets = new List<Asset>();

                for (int i = 1; i <= assetCount; i++)
                {
                    var make = makes[random.Next(makes.Length)];
                    var model = models[random.Next(models.Length)];
                    var year = random.Next(2018, 2025);
                    var purchaseDate = DateTime.UtcNow.AddMonths(-random.Next(1, 48));
                    var value = random.Next(15000, 75000) + random.Next(100, 99) / 100m;
                    
                    var asset = new Asset
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenant.Id,
                        AssetTag = $"{tenant.Subdomain?.ToUpper()}-{year}-{i:D4}",
                        Make = make,
                        Model = model,
                        Year = year,
                        SerialNumber = $"SN{random.Next(10000000, 99999999)}",
                        Value = value,
                        Status = statuses[random.Next(statuses.Length)],
                        PurchaseDate = purchaseDate,
                        CreatedAt = purchaseDate,
                        LastUpdatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        Description = $"{year} {make} {model} - {tenant.Name} asset #{i}",
                        Location = $"Warehouse {random.Next(1, 10)}, Section {random.Next(1, 20)}",
                        Notes = random.Next(0, 10) > 7 ? "High value asset - requires special insurance" : null
                    };

                    assets.Add(asset);
                }

                await context.Assets.AddRangeAsync(assets);
                logger.LogInformation("Added {Count} assets for tenant: {Tenant}", assetCount, tenant.Name);
            }

            await context.SaveChangesAsync();
        }
    }

    private static string GeneratePassword(string role)
    {
        // Generate password based on role with proper complexity
        return role switch
        {
            "Admin" => "Admin@123456!",
            "Financer" => "Finance@123!",
            "Insurer" => "Insurer@123!",
            "Broker" => "Broker@123!",
            "Auditor" => "Audit@123!",
            _ => "Password@123!"
        };
    }

    private static string GetRandomColor()
    {
        var random = new Random();
        var colors = new[]
        {
            "#007bff", // Blue
            "#28a745", // Green
            "#dc3545", // Red
            "#ffc107", // Yellow
            "#17a2b8", // Teal
            "#6f42c1", // Purple
            "#fd7e14", // Orange
            "#20c997", // Mint
            "#e83e8c", // Pink
            "#343a40"  // Dark gray
        };
        
        return colors[random.Next(colors.Length)];
    }
}