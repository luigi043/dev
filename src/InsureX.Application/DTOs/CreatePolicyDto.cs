using System;
using System.ComponentModel.DataAnnotations;

namespace InsureX.Application.DTOs { 
    public class CreatePolicyDto
    {
        [Required(ErrorMessage = "Policy Number is required")]
        [StringLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public int AssetId { get; set; }

        [Required]
        [StringLength(20)]
        public string InsurerCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string InsurerName { get; set; } = string.Empty;

        [Required]
        public string PolicyType { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999.99)]
        public decimal SumInsured { get; set; }

        [Required]
        [Range(0.01, 999999999.99)]
        public decimal Premium { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RenewalDate { get; set; }

        public string? CoverageDetails { get; set; }
        public string? Notes { get; set; }
       
    }
 }
