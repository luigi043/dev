namespace InsureX.Application.DTOs;

public class AssetSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? ComplianceStatus { get; set; }
    public int? Year { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public string? SortBy { get; set; }
    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}