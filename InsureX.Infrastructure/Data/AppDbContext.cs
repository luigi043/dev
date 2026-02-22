using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Infrastructure.Tenant;
using TenantEntity = InsureX.Domain.Entities.Tenant;  // Add this alias to fix namespace conflict

namespace InsureX.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Use the alias here
    public DbSet<TenantEntity> Tenants { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyClaim> PolicyClaims { get; set; }
    public DbSet<ComplianceRule> ComplianceRules { get; set; }
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
    public DbSet<ComplianceAlert> ComplianceAlerts { get; set; }
    public DbSet<ComplianceHistory> ComplianceHistories { get; set; }
    public DbSet<ComplianceDashboard> ComplianceDashboards { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global query filter for multi-tenancy
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantFilter))
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(this, new object[] { builder });
            }
        }

        // Configure relationships
        builder.Entity<Policy>(entity =>
        {
            entity.HasOne(p => p.Asset)
                .WithMany(a => a.Policies)
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.PolicyNumber).IsUnique();
            entity.HasIndex(p => new { p.TenantId, p.EndDate });
        });

        builder.Entity<PolicyClaim>(entity =>
        {
            entity.HasOne(c => c.Policy)
                .WithMany()
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add configuration for Compliance entities
        builder.Entity<ComplianceCheck>(entity =>
        {
            entity.HasOne(c => c.Asset)
                .WithMany()
                .HasForeignKey(c => c.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Rule)
                .WithMany()
                .HasForeignKey(c => c.RuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(c => new { c.AssetId, c.CheckDate });
        });

        builder.Entity<ComplianceAlert>(entity =>
        {
            entity.HasOne(a => a.Asset)
                .WithMany()
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.AssetId, a.Status, a.Severity });
        });

        builder.Entity<ComplianceHistory>(entity =>
        {
            entity.HasOne(h => h.Asset)
                .WithMany()
                .HasForeignKey(h => h.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new { h.AssetId, h.ChangeDate });
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(l => new { l.EntityId, l.EntityType });
            entity.HasIndex(l => l.Timestamp);
        });
    }

    private void SetTenantFilter<T>(ModelBuilder builder) where T : class, ITenantScoped
    {
        builder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantContext.GetCurrentTenantId());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;
        var currentUser = _tenantContext.GetCurrentUserId();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = currentUser ?? "system";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser ?? "system";
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}