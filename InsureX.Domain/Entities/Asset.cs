using InsureX.Domain.Interfaces;

namespace InsureX.Domain.Entities;

public class Asset : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? VIN { get; set; }
    public string Status { get; set; } = AssetStatusValues.Active;
    public string ComplianceStatus { get; set; } = ComplianceStatusValues.Pending;
    public decimal? InsuredValue { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public string? Notes { get; set; }
    public string? AssetType { get; set; }
    public int? ComplianceScore { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
    public virtual ICollection<ComplianceResult> ComplianceResults { get; set; } = new List<ComplianceResult>();
}

// Static classes for constant values
public static class AssetStatusValues
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";
    public const string Sold = "Sold";
    public const string Stolen = "Stolen";
    public const string Totaled = "Totaled";
    public const string Pending = "Pending";
    public const string Deleted = "Deleted";
}

public static class ComplianceStatusValues
{
    public const string Compliant = "Compliant";
    public const string NonCompliant = "NonCompliant";
    public const string Pending = "Pending";
    public const string UnderReview = "UnderReview";
    public const string Expired = "Expired";
}