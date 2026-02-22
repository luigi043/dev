namespace InsureX.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }  // This is already int - good!
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public int? UserId { get; set; }  // Changed from string? to int? to match ApplicationUser Id
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // For alerts
    public string? Status { get; set; }
    public string? Severity { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}