using Microsoft.AspNetCore.Identity;
using InsureX.Domain.Interfaces;

namespace InsureX.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, ITenantScoped
{
    // Tenant relationship
    public Guid TenantId { get; set; }
    
    // User profile
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsEmailConfirmed => EmailConfirmed;
    public bool IsPhoneConfirmed => PhoneNumberConfirmed;
    
    // Activity tracking
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int AccessFailedCount { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Two-factor authentication
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    
    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<UserRole>? UserRoles { get; set; }
    public virtual ICollection<UserClaim>? Claims { get; set; }
    public virtual ICollection<UserLogin>? Logins { get; set; }
    public virtual ICollection<UserToken>? Tokens { get; set; }
    
    // Methods
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
    
    public class UserRole : IdentityUserRole<Guid>
{
    public virtual ApplicationUser? User { get; set; }
    public virtual Role? Role { get; set; }
}

public class Role : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserRole>? UserRoles { get; set; }
    public virtual ICollection<RoleClaim>? RoleClaims { get; set; }
}

public class RoleClaim : IdentityRoleClaim<Guid>
{
    public virtual Role? Role { get; set; }
}


public class UserClaim : IdentityUserClaim<Guid>
{
    public virtual ApplicationUser? User { get; set; }
}
public class UserLogin : IdentityUserLogin<Guid>
{
    public virtual ApplicationUser? User { get; set; }
}

public class UserToken : IdentityUserToken<Guid>
{
    public virtual ApplicationUser? User { get; set; }
}
    public bool CanLogin()
    {
        return IsActive && !(DeletedAt.HasValue) && !LockoutEnabled;
    }
}