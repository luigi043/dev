using System.ComponentModel.DataAnnotations;

namespace InsureX.Domain.Entities;

public class ComplianceCheck : BaseEntity
{
    public Guid AssetId { get; set; }
    public DateTime CheckDate { get; set; }
    public bool IsCompliant { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Asset Asset { get; set; }
}