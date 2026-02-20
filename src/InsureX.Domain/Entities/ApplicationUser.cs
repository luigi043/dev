using System;

namespace InsureX.Domain.Entities;

public class ApplicationUser
{
    // Identity uses string ids; keep Id as string in the domain model to match Identity.
    public string Id { get; set; } = string.Empty;

    public Guid TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation property
    public virtual Tenant? Tenant { get; set; }
}