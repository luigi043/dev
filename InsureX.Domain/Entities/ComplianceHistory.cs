namespace InsureX.Domain.Entities;

public class ComplianceHistory : BaseEntity
{
    public Guid AssetId { get; set; }
    public DateTime ChangeDate { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    
    public virtual Asset Asset { get; set; }
}