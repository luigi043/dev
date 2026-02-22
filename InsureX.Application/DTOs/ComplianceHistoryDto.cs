using System;

namespace InsureX.Application.DTOs;

public class ComplianceHistoryDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public DateTime ChangeDate { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public int FromScore { get; set; }
    public int ToScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
    public string? Evidence { get; set; }
}
