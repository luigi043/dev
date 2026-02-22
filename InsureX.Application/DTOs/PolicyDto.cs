namespace InsureX.Application.DTOs;

public class PolicyDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }  // Changed from Guid
    public string PolicyNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string InsurerCode { get; set; } = string.Empty;
    public string InsurerName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public decimal SumInsured { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public int DaysToExpiry => (int)(EndDate - DateTime.UtcNow).TotalDays;
    public bool IsExpiringSoon => DaysToExpiry > 0 && DaysToExpiry <= 30;
    public bool IsExpired => DaysToExpiry <= 0;
    public string? CoverageDetails { get; set; }
    public int? ClaimsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class CreatePolicyDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public int AssetId { get; set; }
    public string InsurerCode { get; set; } = string.Empty;
    public string InsurerName { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public decimal SumInsured { get; set; }
    public decimal Premium { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? CoverageDetails { get; set; }
    public string? Exclusions { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePolicyDto : CreatePolicyDto
{
    public int Id { get; set; }
}

public class PolicySearchDto
{
    public string? SearchTerm { get; set; }
    public int? AssetId { get; set; }
    public string? InsurerCode { get; set; }
    public string? PolicyType { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? ExpiringOnly { get; set; }
    public bool? ExpiredOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortBy { get; set; } = "EndDate";
    public string SortDir { get; set; } = "asc";
}

public class PolicySummaryDto
{
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int ExpiringPolicies { get; set; }
    public int ExpiredPolicies { get; set; }
    public decimal TotalSumInsured { get; set; }
    public decimal TotalPremium { get; set; }
    public Dictionary<string, int> PoliciesByType { get; set; } = new();
    public Dictionary<string, int> PoliciesByInsurer { get; set; } = new();
}