using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs;

public class AssetDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string VIN { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ComplianceStatus { get; set; } = string.Empty;
    public decimal? InsuredValue { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
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

    [Range(1900, 2100, ErrorMessage = "Please enter a valid year")]
    public int Year { get; set; }

    [StringLength(50)]
    public string? SerialNumber { get; set; }

    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN must be 17 characters")]
    public string? VIN { get; set; }

    [Range(0, 999999999.99, ErrorMessage = "Please enter a valid amount")]
    public decimal? InsuredValue { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LastInspectionDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdateAssetDto : CreateAssetDto
{
    public int Id { get; set; }
}

public class AssetSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? ComplianceStatus { get; set; }
    public int? Year { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortBy { get; set; } = "CreatedAt";
    public string SortDir { get; set; } = "desc";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}