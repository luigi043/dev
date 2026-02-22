using System;
using System.Collections.Generic;
using InsureX.Domain.Entities;

namespace InsureX.Application.DTOs;

public class ComplianceResult
{
    public Guid AssetId { get; set; }
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public DateTime NextCheckDue { get; set; }
    public List<ComplianceAlert> Alerts { get; set; } = new();
    public List<string> Violations { get; set; } = new();
    public double Score { get; set; }
}

public class ComplianceCheckResultDto
{
    public Guid AssetId { get; set; }
    public bool IsCompliant { get; set; }
    public ComplianceSeverity Severity { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
    public DateTime CheckedAt { get; set; }
    public double Score { get; set; }
}