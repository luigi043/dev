using InsureX.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InsureX.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles
        string[] roles = { "Admin", "Financer", "Insurer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default tenant
        var defaultTenant = await context.Tenants.FindAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Default Tenant",
                Subdomain = "default",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await context.Tenants.AddAsync(defaultTenant);
            await context.SaveChangesAsync();
        }

        // Create admin user
        var adminEmail = "admin@insurex.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                TenantId = defaultTenant.Id,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}