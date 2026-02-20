using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class Policy : BaseEntity
{
    public string PolicyNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string InsurerCode { get; set; } = string.Empty;
    public string InsurerName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty; // Comprehensive, Third Party, etc.
    public decimal SumInsured { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Expired, Cancelled, Pending
    public string PaymentStatus { get; set; } = string.Empty; // Paid, Pending, Overdue
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public string? Documents { get; set; } // JSON array of document URLs
    public DateTime? LastClaimDate { get; set; }
    public int? ClaimsCount { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public virtual Asset? Asset { get; set; }
}

public class PolicyClaim
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public DateTime ClaimDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimReference { get; set; }
    public DateTime? SettlementDate { get; set; }
    public decimal? SettlementAmount { get; set; }
}