using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

public class ComplianceAlertDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ComplianceCheckResultDto
{
    public Guid AssetId { get; set; }
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
    public DateTime NextCheckDue { get; set; }
    public List<ComplianceAlertDto> ActiveAlerts { get; set; } = new();
    public List<string> Violations { get; set; } = new();
    public double Score { get; set; }
    public string Severity { get; set; } = "Info";
}

public class ComplianceResultDto
{
    public Guid AssetId { get; set; }
    public bool IsCompliant { get; set; }
    public ComplianceCheckResultDto? LatestCheck { get; set; }
    public List<ComplianceAlertDto> Alerts { get; set; } = new();
    public double ComplianceRate { get; set; }
}