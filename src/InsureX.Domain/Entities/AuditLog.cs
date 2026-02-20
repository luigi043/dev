using System;
using InsureX.Domain.Entities;
namespace InsureX.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? UserName { get; set; }
    // Identity uses string ids; store user id as string to match IdentityUser.Id
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Status { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}