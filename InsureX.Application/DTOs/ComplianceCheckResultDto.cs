using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

public class ComplianceCheckResultDto
{
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CheckedAt { get; set; }
    public string Findings { get; set; } = string.Empty;
    public string? Recommendations { get; set; }
    public DateTime? NextCheckDate { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
}
