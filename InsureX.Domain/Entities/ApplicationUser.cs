using Microsoft.AspNetCore.Identity;
using InsureX.Domain.Interfaces;

namespace InsureX.Domain.Entities;

public class ApplicationUser : IdentityUser<int>, ITenantScoped  // Changed to IdentityUser<int>
{
    // Tenant relationship - using int
    public int TenantId { get; set; }
    
    // User profile
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Activity tracking
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    
    // Audit tracking
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    
    public void UpdateLastLogin(string? ipAddress = null)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
    }
    
    public void Activate(string? updatedBy = null)
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
    
    public void Deactivate(string? updatedBy = null)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
    
    public void SoftDelete(string? deletedBy = null)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        IsActive = false;
    }
    
    public bool CanLogin()
    {
        return IsActive && !(DeletedAt.HasValue) && !LockoutEnabled;
    }
}