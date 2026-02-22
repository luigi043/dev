using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs;

public class AssetSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? ComplianceStatus { get; set; }
    public string? Keyword { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortBy { get; set; } = "CreatedAt";
    public string SortDir { get; set; } = "desc";
    
}

