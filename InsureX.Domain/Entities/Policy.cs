using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities;

public class Policy : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string InsuredEmail { get; set; } = string.Empty;
    public string InsuredPhone { get; set; } = string.Empty;
    
    public PolicyType PolicyType { get; set; }
    public PolicyStatus Status { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    
    public decimal Premium { get; set; }
    public decimal CoverageAmount { get; set; }
    public string Deductible { get; set; } = string.Empty;
    
    public string InsurerCompany { get; set; } = string.Empty;
    public string InsurerPolicyNumber { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; }
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public virtual ICollection<PolicyDocument> Documents { get; set; } = new List<PolicyDocument>();
    public virtual ICollection<PolicyClaim> Claims { get; set; } = new List<PolicyClaim>();
}

public enum PolicyType
{
    Auto = 1,
    Home = 2,
    Life = 3,
    Health = 4,
    Business = 5,
    Equipment = 6,
    Liability = 7
}

public enum PolicyStatus
{
    Pending = 1,
    Active = 2,
    Expired = 3,
    Cancelled = 4,
    Renewed = 5,
    Lapsed = 6
}