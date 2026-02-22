using System;
using System.Collections.Generic;
using InsureX.Domain.Interfaces;

namespace InsureX.Domain.Entities;

public class Asset : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }  // This was missing!
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? VIN { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ComplianceStatus { get; set; } = string.Empty;
    public decimal? InsuredValue { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public string? Notes { get; set; }
    public string? AssetType { get; set; }
    public int? ComplianceScore { get; set; }
    
    // Navigation properties
    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
}