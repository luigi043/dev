namespace InsureX.Domain.Entities;

public class ComplianceAlert : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? AssetId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    public virtual Tenant Tenant { get; set; }
    public virtual Asset? Asset { get; set; }
}