using System;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs;

public class CreateAssetDto
{
    [Required]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int? Year { get; set; }

    [StringLength(50)]
    public string? SerialNumber { get; set; }

    [StringLength(17, MinimumLength = 17)]
    public string? VIN { get; set; }

    [Range(0, 999999999.99)]
    public decimal? InsuredValue { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}