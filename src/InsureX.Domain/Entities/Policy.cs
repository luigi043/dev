using System;
using System.Collections.Generic;

namespace InsureX.Domain.Entities
{
    public class Policy : BaseEntity
    {
       public string? Notes { get; set; }

        // Navigation property
        public virtual Asset? Asset { get; set; }
        public virtual ICollection<PolicyClaim> Claims { get; set; } = new List<PolicyClaim>();
    }

    public class PolicyClaim
    {
       public Guid PolicyId { get; set; }               // Foreign key
        public DateTime ClaimDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ClaimReference { get; set; }
        public DateTime? SettlementDate { get; set; }
        public decimal? SettlementAmount { get; set; }

        // Navigation property
        public virtual Policy? Policy { get; set; }
    }
}