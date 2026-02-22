using System;

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
    public DateTime? DueDate { get; set; }
}