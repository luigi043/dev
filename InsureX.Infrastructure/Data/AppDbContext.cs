using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Reflection;

namespace InsureX.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<
    ApplicationUser,      // User type
    Role,                 // Role type
    int,                  // Key type (int)
    UserClaim,           // User claim type
    UserRole,            // User role type
    UserLogin,           // User login type
    RoleClaim,           // Role claim type
    UserToken            // User token type
>
{
    private readonly ITenantContext? _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // Core Domain Entities
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyClaim> PolicyClaims { get; set; }
    
    // Compliance Entities
    public DbSet<ComplianceRule> ComplianceRules { get; set; }
    public DbSet<ComplianceCheck> ComplianceChecks { get; set; }
    public DbSet<ComplianceAlert> ComplianceAlerts { get; set; }
    public DbSet<ComplianceHistory> ComplianceHistories { get; set; }
    public DbSet<ComplianceDashboard> ComplianceDashboards { get; set; }
    public DbSet<ComplianceReport> ComplianceReports { get; set; }
    
    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ===== TENANT CONFIGURATION =====
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.HasIndex(t => t.Subdomain).IsUnique();
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Subdomain).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // ===== IDENTITY CONFIGURATION (with int keys) =====
        
        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            
            entity.HasIndex(u => u.TenantId);
            entity.HasIndex(u => u.IsActive);
            entity.HasIndex(u => u.LastLoginAt);
            
            // Soft delete filter
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        // Role configuration
        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.Description).HasMaxLength(500);
            
            // Soft delete filter
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // UserRole configuration
        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RoleClaim configuration
        builder.Entity<RoleClaim>(entity =>
        {
            entity.ToTable("RoleClaims");
            entity.HasKey(rc => rc.Id);
            entity.Property(rc => rc.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserClaim configuration
        builder.Entity<UserClaim>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.HasKey(uc => uc.Id);
            entity.Property(uc => uc.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(uc => uc.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserLogin configuration
        builder.Entity<UserLogin>(entity =>
        {
            entity.ToTable("UserLogins");
            entity.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
            
            entity.HasOne(ul => ul.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserToken configuration
        builder.Entity<UserToken>(entity =>
        {
            entity.ToTable("UserTokens");
            entity.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            
            entity.HasOne(ut => ut.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== ASSET CONFIGURATION =====
        builder.Entity<Asset>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.TenantId, a.AssetTag }).IsUnique();
            entity.HasIndex(a => new { a.TenantId, a.VIN });
            entity.HasIndex(a => new { a.TenantId, a.SerialNumber });
            entity.HasIndex(a => a.ComplianceStatus);
            entity.HasIndex(a => a.Status);
            
            entity.Property(a => a.AssetTag).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Make).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Model).IsRequired().HasMaxLength(100);
            entity.Property(a => a.VIN).HasMaxLength(50);
            entity.Property(a => a.SerialNumber).HasMaxLength(100);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(50);
            entity.Property(a => a.ComplianceStatus).IsRequired().HasMaxLength(50);
            entity.Property(a => a.AssetType).HasMaxLength(100);
            entity.Property(a => a.InsuredValue).HasPrecision(18, 2);
            entity.Property(a => a.Notes).HasMaxLength(1000);
            entity.Property(a => a.CreatedBy).HasMaxLength(100);
            entity.Property(a => a.UpdatedBy).HasMaxLength(100);
            
            // Relationships
            entity.HasMany(a => a.Policies)
                .WithOne(p => p.Asset)
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(a => a.ComplianceResults)
                .WithOne(c => c.Asset)
                .HasForeignKey(c => c.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Soft delete filter
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ===== POLICY CONFIGURATION =====
        builder.Entity<Policy>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(p => p.Asset)
                .WithMany(a => a.Policies)
                .HasForeignKey(p => p.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.PolicyNumber).IsUnique();
            entity.HasIndex(p => new { p.TenantId, p.EndDate });
            
            entity.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
            entity.Property(p => p.InsurerName).HasMaxLength(200);
            entity.Property(p => p.PolicyType).HasMaxLength(100);
            entity.Property(p => p.SumInsured).HasPrecision(18, 2);
            entity.Property(p => p.Premium).HasPrecision(18, 2);
            entity.Property(p => p.CreatedBy).HasMaxLength(100);
            entity.Property(p => p.UpdatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===== COMPLIANCE RULE CONFIGURATION =====
        builder.Entity<ComplianceRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.TenantId, r.RuleCode }).IsUnique();
            entity.HasIndex(r => r.RuleType);
            entity.HasIndex(r => r.IsActive);
            
            entity.Property(r => r.RuleName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.RuleCode).IsRequired().HasMaxLength(50);
            entity.Property(r => r.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.Expression).HasColumnType("nvarchar(max)");
            entity.Property(r => r.ApplicableAssetTypes).HasMaxLength(500);
            entity.Property(r => r.CreatedBy).HasMaxLength(100);
            entity.Property(r => r.UpdatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // ===== COMPLIANCE CHECK CONFIGURATION =====
        builder.Entity<ComplianceCheck>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(c => c.Asset)
                .WithMany(a => a.ComplianceResults)
                .HasForeignKey(c => c.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Rule)
                .WithMany(r => r.ComplianceChecks)
                .HasForeignKey(c => c.RuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.AssetId, c.CheckDate });
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.NextCheckDate);

            entity.Property(c => c.Status).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Findings).HasColumnType("nvarchar(max)");
            entity.Property(c => c.Recommendations).HasColumnType("nvarchar(max)");
            entity.Property(c => c.CheckedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // Apply global tenant filters if tenant context exists
        if (_tenantContext?.HasTenant == true)
        {
            ApplyGlobalTenantFilters(builder);
        }
    }

    private void ApplyGlobalTenantFilters(ModelBuilder builder)
    {
        var tenantId = _tenantContext!.GetCurrentTenantId();

        // Apply to all ITenantScoped entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var tenantProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
                var tenantConstant = Expression.Constant(tenantId);
                var condition = Expression.Equal(tenantProperty, tenantConstant);
                var lambda = Expression.Lambda(condition, parameter);
                
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var now = DateTime.UtcNow;
        var currentUser = GetCurrentUserId();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser;
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    break;
                    
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = currentUser;
                    entry.Entity.IsDeleted = true;
                    break;
            }
        }
    }

    private string GetCurrentUserId()
    {
        // Try to get from tenant context first
        if (_tenantContext != null)
        {
            var userId = _tenantContext.GetCurrentUserId();
            if (!string.IsNullOrEmpty(userId))
                return userId;
        }

        // Fallback to system
        return "system";
    }
}