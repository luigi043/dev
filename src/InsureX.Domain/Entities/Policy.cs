using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class Policy : BaseEntity
{
    public string PolicyNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string InsurerCode { get; set; } = string.Empty;
    public string InsurerName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public decimal SumInsured { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public string? Documents { get; set; }
    public DateTime? LastClaimDate { get; set; }
    public int? ClaimsCount { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual Asset? Asset { get; set; }
}