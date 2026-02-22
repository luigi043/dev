using System;

namespace InsureX.Domain.Entities;

public class PolicyClaim : BaseEntity
{
    public int PolicyId { get; set; }
    public DateTime ClaimDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimReference { get; set; }
    public DateTime? SettlementDate { get; set; }
    public decimal? SettlementAmount { get; set; }
    
    // Navigation property
    public virtual Policy? Policy { get; set; }
}