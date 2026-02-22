namespace InsureX.Application.DTOs;

public class AssetSearchDto
{
    private int _page = 1;
    private int _pageSize = 25;
    private string? _searchTerm;
    private string? _sortBy = "createdat";
    private string? _sortDir = "desc";

    // Search criteria
    public string? SearchTerm
    {
        get => _searchTerm;
        set => _searchTerm = value?.Trim();
    }
    
    public string? Keyword
    {
        get => SearchTerm;
        set => SearchTerm = value;
    }
    
    public string? Status { get; set; }
    public string? ComplianceStatus { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
    // Pagination
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 25 : value > 100 ? 100 : value;
    }

    // Sorting
    public string? SortBy
    {
        get => _sortBy;
        set => _sortBy = string.IsNullOrWhiteSpace(value) ? "createdat" : value.ToLower();
    }

    public string SortDir
    {
        get => _sortDir;
        set => _sortDir = value?.ToLower() == "asc" ? "asc" : "desc";
    }
}