using System.ComponentModel.DataAnnotations;
using InsureX.Application.Common.Constants;

namespace InsureX.Application.DTOs;

public class AssetDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }  // Changed from Guid
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? VIN { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ComplianceStatus { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public decimal? InsuredValue { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public string? Notes { get; set; }
    public string? AssetType { get; set; }
    public int? ComplianceScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreateAssetDto
{
    [Required(ErrorMessage = "Asset Tag is required")]
    [StringLength(50, ErrorMessage = "Asset Tag cannot exceed 50 characters")]
    public string AssetTag { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Make is required")]
    [StringLength(100, ErrorMessage = "Make cannot exceed 100 characters")]
    public string Make { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Model is required")]
    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    public string Model { get; set; } = string.Empty;
    
    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
    public int Year { get; set; } = DateTime.UtcNow.Year;
    
    [StringLength(100)]
    public string? SerialNumber { get; set; }
    
    [StringLength(50)]
    public string? VIN { get; set; }
    
    public decimal? Value { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Insured value must be a positive number")]
    public decimal? InsuredValue { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LastInspectionDate { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? AssetType { get; set; }
}

public class UpdateAssetDto
{
    [Required]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Asset Tag is required")]
    [StringLength(50, ErrorMessage = "Asset Tag cannot exceed 50 characters")]
    public string AssetTag { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Make is required")]
    [StringLength(100, ErrorMessage = "Make cannot exceed 100 characters")]
    public string Make { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Model is required")]
    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    public string Model { get; set; } = string.Empty;
    
    [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
    public int Year { get; set; }
    
    [StringLength(100)]
    public string? SerialNumber { get; set; }
    
    [StringLength(50)]
    public string? VIN { get; set; }
    
    public decimal? Value { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Insured value must be a positive number")]
    public decimal? InsuredValue { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? LastInspectionDate { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? AssetType { get; set; }
}