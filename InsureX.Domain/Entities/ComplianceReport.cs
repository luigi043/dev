using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsureX.Domain.Entities;

public class ComplianceReport : BaseEntity, ITenantScoped
{
    public int TenantId { get; set; }  // Changed from Guid to int

    [Required]
    [StringLength(200)]
    public string ReportName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ReportType { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Format { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string GeneratedBy { get; set; } = string.Empty;

    [StringLength(500)]
    public string? FilePath { get; set; }

    public long? FileSize { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Parameters { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Summary { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public string? ErrorMessage { get; set; }
    public DateTime? DownloadedAt { get; set; }
    public int DownloadCount { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
}