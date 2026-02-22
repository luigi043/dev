using System;

namespace InsureX.Application.DTOs
{
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
}
