using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs
{
    public class PolicyStatistics
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
}
