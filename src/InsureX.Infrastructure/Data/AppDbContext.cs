using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;

namespace InsureX.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Policy> Policies { get; set; } = null!;
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<ApplicationUser> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
