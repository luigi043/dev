using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs
{
    public class PolicyDto
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
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
        public string CreatedBy { get; set; } = string.Empty;
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
}
