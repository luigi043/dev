using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs;

public class AssetDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public int? Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? VIN { get; set; }

    public string? Status { get; set; }
    public string? ComplianceStatus { get; set; }

    public decimal? InsuredValue { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime? LastInspectionDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public string? Notes { get; set; }
}





