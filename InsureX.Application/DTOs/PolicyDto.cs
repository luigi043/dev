using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs;

public class PolicyDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string InsuredEmail { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Premium { get; set; }
    public decimal CoverageAmount { get; set; }
    public string InsurerCompany { get; set; } = string.Empty;
    public int DaysToExpiry => (ExpiryDate - DateTime.Today).Days;
    public bool IsExpiringSoon => DaysToExpiry > 0 && DaysToExpiry <= 30;
    public bool IsExpired => DaysToExpiry < 0;
}

public class CreatePolicyDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public string InsuredEmail { get; set; } = string.Empty;
    public string InsuredPhone { get; set; } = string.Empty;
    public PolicyType PolicyType { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Premium { get; set; }
    public decimal CoverageAmount { get; set; }
    public string Deductible { get; set; } = string.Empty;
    public string InsurerCompany { get; set; } = string.Empty;
    public string InsurerPolicyNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<Guid> AssetIds { get; set; } = new();
}

public class UpdatePolicyDto : CreatePolicyDto
{
    public Guid Id { get; set; }
    public PolicyStatus Status { get; set; }
}

public class PolicyDetailDto : PolicyDto
{
    public string InsuredPhone { get; set; } = string.Empty;
    public string Deductible { get; set; } = string.Empty;
    public string InsurerPolicyNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<AssetDto> Assets { get; set; } = new();
    public List<PolicyDocumentDto> Documents { get; set; } = new();
    public List<PolicyClaimDto> Claims { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PolicyDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

public class PaymentDto
{
    public string Status { get; set; } = string.Empty; // Paid, Pending, Failed
    public DateTime PaymentDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
public class PolicyClaimDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime DateOfLoss { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PolicySearchDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? PolicyType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}