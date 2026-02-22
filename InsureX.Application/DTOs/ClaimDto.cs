namespace InsureX.Application.DTOs;

public class ClaimDto
{
    public int Id { get; set; }
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime ClaimDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimReference { get; set; }
    public DateTime? SettlementDate { get; set; }
    public decimal? SettlementAmount { get; set; }
}

public class CreateClaimDto
{
    public int PolicyId { get; set; }
    public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
    public decimal ClaimAmount { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimReference { get; set; }
}