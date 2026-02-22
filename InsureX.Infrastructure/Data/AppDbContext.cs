using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Reflection;

namespace InsureX.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<
    ApplicationUser, 
    Role, 
    Guid, 
    UserClaim, 
    UserRole, 
    UserLogin, 
    RoleClaim, 
    UserToken>
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
            entity.HasIndex(t => t.Subdomain).IsUnique();
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Subdomain).HasMaxLength(100);
            entity.Property(t => t.CreatedBy).HasMaxLength(100);
            entity.Property(t => t.UpdatedBy).HasMaxLength(100);
            entity.Property(t => t.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // ===== IDENTITY CONFIGURATION =====
        
        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            
            entity.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.CreatedBy).HasMaxLength(100);
            entity.Property(u => u.UpdatedBy).HasMaxLength(100);
            entity.Property(u => u.DeletedBy).HasMaxLength(100);
            
            entity.HasIndex(u => u.TenantId);
            entity.HasIndex(u => u.IsActive);
            entity.HasIndex(u => u.LastLoginAt);
            
            // Soft delete filter
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        // Role configuration
        builder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.Property(r => r.CreatedBy).HasMaxLength(100);
            entity.Property(r => r.UpdatedBy).HasMaxLength(100);
            entity.Property(r => r.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // UserRole configuration
        builder.Entity<UserRole>(entity =>
        {
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
            entity.HasKey(rc => rc.Id);
            
            entity.HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserClaim configuration
        builder.Entity<UserClaim>(entity =>
        {
            entity.HasKey(uc => uc.Id);
            
            entity.HasOne(uc => uc.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserLogin configuration
        builder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
            
            entity.HasOne(ul => ul.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserToken configuration
        builder.Entity<UserToken>(entity =>
        {
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
            
            entity.HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.TenantId, a.AssetTag }).IsUnique();
            entity.HasIndex(a => a.SerialNumber);
            entity.HasIndex(a => a.Status);
            
            entity.Property(a => a.AssetTag).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Make).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Model).IsRequired().HasMaxLength(100);
            entity.Property(a => a.SerialNumber).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Value).HasPrecision(18, 2);
            entity.Property(a => a.CreatedBy).HasMaxLength(100);
            entity.Property(a => a.UpdatedBy).HasMaxLength(100);
            entity.Property(a => a.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ===== POLICY CONFIGURATION =====
        builder.Entity<Policy>(entity =>
        {
            entity.HasKey(p => p.Id);
            
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
            entity.Property(p => p.CoverageType).HasMaxLength(100);
            entity.Property(p => p.Premium).HasPrecision(18, 2);
            entity.Property(p => p.CoverageAmount).HasPrecision(18, 2);
            entity.Property(p => p.Deductible).HasPrecision(18, 2);
            entity.Property(p => p.CreatedBy).HasMaxLength(100);
            entity.Property(p => p.UpdatedBy).HasMaxLength(100);
            entity.Property(p => p.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===== POLICY CLAIM CONFIGURATION =====
        builder.Entity<PolicyClaim>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.Policy)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.ClaimNumber).IsUnique();
            entity.HasIndex(c => c.Status);
            
            entity.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(50);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.ClaimAmount).HasPrecision(18, 2);
            entity.Property(c => c.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(c => c.Status).HasMaxLength(50);
            entity.Property(c => c.CreatedBy).HasMaxLength(100);
            entity.Property(c => c.UpdatedBy).HasMaxLength(100);
            entity.Property(c => c.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // ===== COMPLIANCE RULE CONFIGURATION =====
        builder.Entity<ComplianceRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            
            entity.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.TenantId, r.RuleCode }).IsUnique();
            entity.HasIndex(r => r.RuleType);
            entity.HasIndex(r => r.IsActive);
            entity.HasIndex(r => r.Priority);
            entity.HasIndex(r => r.EffectiveFrom);
            entity.HasIndex(r => r.EffectiveTo);

            entity.Property(r => r.RuleName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.RuleCode).IsRequired().HasMaxLength(50);
            entity.Property(r => r.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.Condition).HasColumnType("nvarchar(max)");
            entity.Property(r => r.Action).HasColumnType("nvarchar(max)");
            entity.Property(r => r.ApplicablePolicyTypes).HasColumnType("nvarchar(max)");
            entity.Property(r => r.ApplicableAssetTypes).HasColumnType("nvarchar(max)");
            entity.Property(r => r.CustomScript).HasColumnType("nvarchar(max)");
            entity.Property(r => r.MinValue).HasPrecision(18, 2);
            entity.Property(r => r.MaxValue).HasPrecision(18, 2);
            entity.Property(r => r.CreatedBy).HasMaxLength(100);
            entity.Property(r => r.UpdatedBy).HasMaxLength(100);
            entity.Property(r => r.DeletedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // ===== COMPLIANCE CHECK CONFIGURATION =====
        builder.Entity<ComplianceCheck>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.Asset)
                .WithMany()
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
            entity.Property(c => c.Evidence).HasColumnType("nvarchar(max)");
            entity.Property(c => c.CheckedBy).HasMaxLength(100);
            entity.Property(c => c.ErrorMessage).HasMaxLength(500);
            entity.Property(c => c.CreatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // ===== COMPLIANCE ALERT CONFIGURATION =====
        builder.Entity<ComplianceAlert>(entity =>
        {
            entity.HasKey(a => a.Id);
            
            entity.HasOne(a => a.Asset)
                .WithMany()
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Rule)
                .WithMany(r => r.Alerts)
                .HasForeignKey(a => a.RuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.ComplianceCheck)
                .WithMany()
                .HasForeignKey(a => a.ComplianceCheckId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.ParentAlert)
                .WithMany(a => a.ChildAlerts)
                .HasForeignKey(a => a.ParentAlertId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.TenantId, a.Status });
            entity.HasIndex(a => new { a.TenantId, a.Severity });
            entity.HasIndex(a => a.DueDate);
            entity.HasIndex(a => a.AlertType);

            entity.Property(a => a.AlertType).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(2000);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(50);
            entity.Property(a => a.AcknowledgedBy).HasMaxLength(100);
            entity.Property(a => a.ResolvedBy).HasMaxLength(100);
            entity.Property(a => a.ResolutionNotes).HasMaxLength(1000);
            entity.Property(a => a.Metadata).HasColumnType("nvarchar(max)");
            entity.Property(a => a.CreatedBy).HasMaxLength(100);
            entity.Property(a => a.UpdatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ===== COMPLIANCE HISTORY CONFIGURATION =====
        builder.Entity<ComplianceHistory>(entity =>
        {
            entity.HasKey(h => h.Id);
            
            entity.HasOne(h => h.Asset)
                .WithMany()
                .HasForeignKey(h => h.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.Rule)
                .WithMany(r => r.ComplianceHistories)
                .HasForeignKey(h => h.RuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(h => h.ComplianceCheck)
                .WithMany()
                .HasForeignKey(h => h.ComplianceCheckId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(h => h.Alert)
                .WithMany()
                .HasForeignKey(h => h.AlertId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(h => h.Tenant)
                .WithMany()
                .HasForeignKey(h => h.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(h => new { h.AssetId, h.ChangeDate });
            entity.HasIndex(h => h.ToStatus);

            entity.Property(h => h.FromStatus).IsRequired().HasMaxLength(50);
            entity.Property(h => h.ToStatus).IsRequired().HasMaxLength(50);
            entity.Property(h => h.Reason).HasMaxLength(1000);
            entity.Property(h => h.TriggeredBy).HasMaxLength(100);
            entity.Property(h => h.Evidence).HasColumnType("nvarchar(max)");
            entity.Property(h => h.CreatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(h => !h.IsDeleted);
        });

        // ===== COMPLIANCE DASHBOARD CONFIGURATION =====
        builder.Entity<ComplianceDashboard>(entity =>
        {
            entity.HasKey(d => d.Id);
            
            entity.HasOne(d => d.Tenant)
                .WithMany()
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(d => new { d.TenantId, d.SnapshotDate });
            entity.HasIndex(d => d.IsCurrent);

            entity.Property(d => d.TopIssues).HasColumnType("nvarchar(max)");
            entity.Property(d => d.AssetTypeBreakdown).HasColumnType("nvarchar(max)");
            entity.Property(d => d.RuleTypeBreakdown).HasColumnType("nvarchar(max)");
            entity.Property(d => d.TrendData).HasColumnType("nvarchar(max)");
            entity.Property(d => d.AlertsBySeverity).HasColumnType("nvarchar(max)");
            entity.Property(d => d.Notes).HasMaxLength(500);
            entity.Property(d => d.CreatedBy).HasMaxLength(100);
        });

        // ===== COMPLIANCE REPORT CONFIGURATION =====
        builder.Entity<ComplianceReport>(entity =>
        {
            entity.HasKey(r => r.Id);
            
            entity.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.TenantId, r.GeneratedAt });
            entity.HasIndex(r => r.ReportType);
            entity.HasIndex(r => r.Status);

            entity.Property(r => r.ReportName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.ReportType).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Format).IsRequired().HasMaxLength(20);
            entity.Property(r => r.GeneratedBy).HasMaxLength(100);
            entity.Property(r => r.FilePath).HasMaxLength(500);
            entity.Property(r => r.Status).HasMaxLength(50);
            entity.Property(r => r.Parameters).HasColumnType("nvarchar(max)");
            entity.Property(r => r.Summary).HasColumnType("nvarchar(max)");
            entity.Property(r => r.ErrorMessage).HasMaxLength(500);
            entity.Property(r => r.CreatedBy).HasMaxLength(100);
            
            // Soft delete filter
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        // ===== AUDIT LOG CONFIGURATION =====
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            
            entity.HasIndex(l => new { l.EntityId, l.EntityType });
            entity.HasIndex(l => l.Timestamp);
            entity.HasIndex(l => l.UserId);
            
            entity.Property(l => l.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(l => l.Action).IsRequired().HasMaxLength(50);
            entity.Property(l => l.UserId).HasMaxLength(100);
            entity.Property(l => l.IpAddress).HasMaxLength(50);
            entity.Property(l => l.UserAgent).HasMaxLength(500);
            entity.Property(l => l.OldValues).HasColumnType("nvarchar(max)");
            entity.Property(l => l.NewValues).HasColumnType("nvarchar(max)");
            entity.Property(l => l.Changes).HasColumnType("nvarchar(max)");
        });

        // Apply global tenant filters if tenant context exists
        if (_tenantContext?.TenantId != null)
        {
            ApplyGlobalTenantFilters(builder);
        }
    }

    private void ApplyGlobalTenantFilters(ModelBuilder builder)
    {
        var tenantId = _tenantContext!.TenantId.Value;

        // Apply to all ITenantScoped entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);
                method?.Invoke(this, new object[] { builder });
            }
        }

        // Special handling for ComplianceCheck (needs to go through Asset)
        builder.Entity<ComplianceCheck>()
            .HasQueryFilter(c => !c.IsDeleted && c.TenantId == tenantId);
        
        // Special handling for ComplianceHistory (needs to go through Asset)
        builder.Entity<ComplianceHistory>()
            .HasQueryFilter(h => !h.IsDeleted && h.TenantId == tenantId);
        
        // Special handling for PolicyClaim
        builder.Entity<PolicyClaim>()
            .HasQueryFilter(c => !c.IsDeleted && c.TenantId == tenantId);
        
        // Special handling for AuditLog (no tenant filter)
    }

    private void SetTenantFilter<T>(ModelBuilder builder) where T : class, ITenantScoped
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && e.TenantId == _tenantContext!.TenantId);
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
                    if (entry.Entity is ITenantScoped tenantScoped && tenantScoped.TenantId == Guid.Empty)
                    {
                        tenantScoped.TenantId = _tenantContext?.TenantId ?? Guid.Empty;
                    }
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
}// Configure Asset entity
builder.Entity<Asset>(entity =>
{
    // Primary key
    entity.HasKey(e => e.Id);
    
    // Tenant isolation - critical for multi-tenancy
    entity.HasOne<Tenant>()
        .WithMany()
        .HasForeignKey(e => e.TenantId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Indexes for performance
    entity.HasIndex(e => new { e.TenantId, e.AssetTag }).IsUnique();
    entity.HasIndex(e => new { e.TenantId, e.VIN });
    entity.HasIndex(e => new { e.TenantId, e.SerialNumber });
    entity.HasIndex(e => e.ComplianceStatus);
    entity.HasIndex(e => e.Status);
    
    // Property configurations
    entity.Property(e => e.AssetTag)
        .IsRequired()
        .HasMaxLength(50);
    
    entity.Property(e => e.Make)
        .IsRequired()
        .HasMaxLength(100);
    
    entity.Property(e => e.Model)
        .IsRequired()
        .HasMaxLength(100);
    
    entity.Property(e => e.VIN)
        .HasMaxLength(50);
    
    entity.Property(e => e.SerialNumber)
        .HasMaxLength(100);
    
    entity.Property(e => e.Status)
        .IsRequired()
        .HasMaxLength(50)
        .HasDefaultValue(AssetStatusValues.Active);
    
    entity.Property(e => e.ComplianceStatus)
        .IsRequired()
        .HasMaxLength(50)
        .HasDefaultValue(ComplianceStatusValues.Pending);
    
    entity.Property(e => e.AssetType)
        .HasMaxLength(100);
    
    entity.Property(e => e.InsuredValue)
        .HasPrecision(18, 2);
    
    entity.Property(e => e.Notes)
        .HasMaxLength(1000);
    
    entity.Property(e => e.CreatedBy)
        .HasMaxLength(100);
    
    entity.Property(e => e.UpdatedBy)
        .HasMaxLength(100);
    
    // Relationships
    entity.HasMany(e => e.Policies)
        .WithOne()
        .HasForeignKey("AssetId")
        .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasMany(e => e.ComplianceResults)
        .WithOne(c => c.Asset)
        .HasForeignKey(c => c.AssetId)
        .OnDelete(DeleteBehavior.Cascade);
});