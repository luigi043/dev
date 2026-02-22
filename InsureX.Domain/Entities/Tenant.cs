using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Domain.Entities;

public class Tenant : BaseEntity
{
    public Tenant()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Users = new HashSet<ApplicationUser>();
    }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Subdomain { get; set; }

    [StringLength(255)]
    public string? Domain { get; set; }

    [StringLength(500)]
    public string? DatabaseConnectionString { get; set; }

    [StringLength(50)]
    public string TenantType { get; set; } = "Standard"; // Standard, Enterprise, Trial

    public bool IsActive { get; set; } = true;

    [StringLength(4000)]
    public string? Settings { get; set; } // JSON serialized settings

    public DateTime? SubscriptionExpiry { get; set; }

    public int MaxUsers { get; set; } = 10;

    public int MaxAssets { get; set; } = 1000;

    [StringLength(200)]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public string? LogoUrl { get; set; }

    public string? PrimaryColor { get; set; } = "#007bff";

    public DateTime? LastActivityAt { get; set; }

    // Navigation properties
    public virtual ICollection<ApplicationUser> Users { get; set; }

    // Audit fields
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}