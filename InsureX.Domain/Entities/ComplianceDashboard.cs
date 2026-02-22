namespace InsureX.Domain.Entities;

public class ComplianceDashboard : BaseEntity
{
    public Guid TenantId { get; set; }
    public int TotalAssets { get; set; }
    public int CompliantAssets { get; set; }
    public int NonCompliantAssets { get; set; }
    public int ActiveAlerts { get; set; }
    public double ComplianceRate { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    public virtual Tenant Tenant { get; set; }
}