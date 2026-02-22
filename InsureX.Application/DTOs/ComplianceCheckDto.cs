using System;

namespace InsureX.Application.DTOs;

public class ComplianceCheckDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public int? RuleId { get; set; }
    public DateTime CheckDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Findings { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public string CheckedBy { get; set; } = string.Empty;
    public string? Evidence { get; set; }
    public bool IsAutomatic { get; set; }
}
