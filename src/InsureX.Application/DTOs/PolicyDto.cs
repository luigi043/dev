using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs
{
    public class PolicyDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }

        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        public Guid AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;

        public string InsurerCode { get; set; } = string.Empty;
        public string InsurerName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;

        public decimal SumInsured { get; set; }
        public decimal Premium { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RenewalDate { get; set; }

        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        // Computed properties
        public int DaysToExpiry => (int)(EndDate - DateTime.UtcNow).TotalDays;
        public bool IsExpiringSoon => DaysToExpiry > 0 && DaysToExpiry <= 30;
        public bool IsExpired => DaysToExpiry <= 0;

        public string? CoverageDetails { get; set; }
        public int? ClaimsCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class PolicySearchDto
    {
        public string? SearchTerm { get; set; }
        public Guid? AssetId { get; set; }
        public string? Status { get; set; }
        public bool? ExpiringOnly { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; } = "EndDate";
        public string SortDir { get; set; } = "asc";
    }

    public class ClaimDto
    {
        public Guid Id { get; set; }
        public Guid PolicyId { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public decimal ClaimAmount { get; set; }

        public string ClaimType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime? SettlementDate { get; set; }
        public decimal? SettlementAmount { get; set; }
    }

    public class CreateClaimDto
    {
        [Required]
        public Guid PolicyId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; }

        [Required]
        [Range(0.01, 999_999_999.99)]
        public decimal ClaimAmount { get; set; }

        [Required]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    public class CreatePolicyDto
    {
        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public Guid AssetId { get; set; }

        [Required]
        public string InsurerCode { get; set; } = string.Empty;

        [Required]
        public string InsurerName { get; set; } = string.Empty;

        [Required]
        public string PolicyType { get; set; } = string.Empty;

        [Required]
        public decimal SumInsured { get; set; }

        [Required]
        public decimal Premium { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime? RenewalDate { get; set; }
    }

    public class UpdatePolicyDto : CreatePolicyDto
    {
        [Required]
        public Guid Id { get; set; } // Only here, no duplicates
    }
}