using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using InsureX.Domain.Entities;
using InsureX.Infrastructure.Data;

namespace InsureX.Infrastructure.Services;

public class DataSeeder : IDataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;  // FIXED: Use Role, not IdentityRole
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed Roles (using int IDs)
            string[] roles = { "Admin", "Manager", "User" };
            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new Role 
                    { 
                        Name = roleName,
                        NormalizedName = roleName.ToUpperInvariant()
                    });
                    _logger.LogInformation("Created role: {Role}", roleName);
                }
            }

            // Seed Admin User
            var adminEmail = "admin@insurex.com";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    TenantId = 1  // FIXED: Use int, not Guid
                };

                var result = await _userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                    _logger.LogInformation("Created admin user");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding data");
            throw;
        }
    }
}