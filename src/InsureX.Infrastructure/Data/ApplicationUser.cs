using Microsoft.AspNetCore.Identity;
using InsureX.Domain.Entities;

namespace InsureX.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation property
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}