using Microsoft.AspNetCore.Identity;
using InsureX.Domain.Entities; // Add this if Tenant is in a different namespace, or adjust accordingly

namespace InsureX.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation 
    public virtual Tenant? Tenant { get; set; }
}