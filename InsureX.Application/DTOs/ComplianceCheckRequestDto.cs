using System;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs;

/// <summary>
/// Request to perform a compliance check on an asset
/// </summary>
public class ComplianceCheckRequestDto
{
    /// <summary>
    /// Asset ID to check
    /// </summary>
    [Required]
    public Guid AssetId { get; set; }
    
    /// <summary>
    /// Type of compliance check to perform
    /// </summary>
    [Required]
    public string CheckType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to force a check even if recently performed
    /// </summary>
    public bool ForceCheck { get; set; }
    
    /// <summary>
    /// Optional notes for the check
    /// </summary>
    public string? Notes { get; set; }
}