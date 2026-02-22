using System;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs
{
    public class CreateClaimDto
    {
        [Required]
        public int PolicyId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0.01, 999999999.99)]
        public decimal ClaimAmount { get; set; }

        [Required]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? ClaimReference { get; set; }
    }
}
