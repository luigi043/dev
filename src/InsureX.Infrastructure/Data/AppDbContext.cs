using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;

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

    // DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyClaim> PolicyClaims { get; set; }
    public DbSet<ComplianceRule> ComplianceRules { get; set; }
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
    public DbSet<ComplianceAlert> ComplianceAlerts { get; set; }
    public DbSet<ComplianceHistory> ComplianceHistories { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply tenant filters to all tenant-scoped entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(nameof(SetTenantFilter))
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(this, new object[] { builder });
            }
        }

        // Asset configuration
        builder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.AssetTag).IsUnique();
            entity.HasIndex(a => new { a.TenantId, a.Status });
            
            entity.Property(a => a.AssetTag).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Make).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Model).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Status).HasMaxLength(50);
            entity.Property(a => a.ComplianceStatus).HasMaxLength(50);
        });

        // Policy configuration
        builder.Entity<Policy>(entity =>
        {
            entity.HasIndex(p => p.PolicyNumber).IsUnique();
            entity.HasIndex(p => new { p.TenantId, p.EndDate, p.Status });

            entity.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            entity.Property(p => p.InsurerName).HasMaxLength(200);
            entity.Property(p => p.PolicyType).HasMaxLength(100);
            entity.Property(p => p.Status).HasMaxLength(50);
            entity.Property(p => p.PaymentStatus).HasMaxLength(50);

            entity.HasOne(p => p.Asset)
                .WithMany(a => a.Policies)
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PolicyClaim configuration
        builder.Entity<PolicyClaim>(entity =>
        {
            entity.HasIndex(c => new { c.PolicyId, c.ClaimDate });

            entity.Property(c => c.ClaimType).HasMaxLength(100);
            entity.Property(c => c.Status).HasMaxLength(50);
            entity.Property(c => c.ClaimReference).HasMaxLength(100);

            entity.HasOne(c => c.Policy)
                .WithMany()
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ComplianceRule configuration
        builder.Entity<ComplianceRule>(entity =>
        {
            entity.HasIndex(r => r.RuleCode).IsUnique();
            entity.HasIndex(r => new { r.TenantId, r.RuleType, r.IsActive });

            entity.Property(r => r.RuleName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.RuleCode).IsRequired().HasMaxLength(50);
            entity.Property(r => r.RuleType).HasMaxLength(50);
        });

        // ComplianceCheck configuration
        builder.Entity<ComplianceCheck>(entity =>
        {
            entity.HasIndex(c => new { c.AssetId, c.CheckDate });

            entity.Property(c => c.Status).HasMaxLength(50);
            entity.Property(c => c.CheckedBy).HasMaxLength(100);

            entity.HasOne(c => c.Asset)
                .WithMany()
                .HasForeignKey(c => c.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Rule)
                .WithMany()
                .HasForeignKey(c => c.RuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ComplianceAlert configuration
        builder.Entity<ComplianceAlert>(entity =>
        {
            entity.HasIndex(a => new { a.AssetId, a.Status, a.Severity });

            entity.Property(a => a.AlertType).HasMaxLength(50);
            entity.Property(a => a.Title).HasMaxLength(200);
            entity.Property(a => a.Status).HasMaxLength(50);

            entity.HasOne(a => a.Asset)
                .WithMany()
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(l => new { l.EntityId, l.EntityType });
            entity.HasIndex(l => l.Timestamp);

            entity.Property(l => l.EntityType).HasMaxLength(100);
            entity.Property(l => l.Action).HasMaxLength(100);
            entity.Property(l => l.UserId).HasMaxLength(100);
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
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser ?? "system";
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser ?? "system";
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}