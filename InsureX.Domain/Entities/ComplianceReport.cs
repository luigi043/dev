namespace InsureX.Domain.Entities;

/// <summary>
/// Represents a generated compliance report
/// </summary>
public class ComplianceReport : BaseEntity, ITenantScoped
{
    /// <summary>
    /// Tenant identifier for multi-tenancy isolation
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Name of the report
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ReportName { get; set; } = string.Empty;

    /// <summary>
    /// Type of report: Summary, Detailed, Audit, Custom
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Format of the report: PDF, Excel, CSV, JSON
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Start date for the report period
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End date for the report period
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who generated the report
    /// </summary>
    [StringLength(100)]
    public string GeneratedBy { get; set; } = string.Empty;

    /// <summary>
    /// Path or URL to the stored report file
    /// </summary>
    [StringLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    /// Size of the report file in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// JSON parameters used to generate the report
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Parameters { get; set; }

    /// <summary>
    /// JSON summary of report contents
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Summary { get; set; }

    /// <summary>
    /// Status: Pending, Processing, Completed, Failed
    /// </summary>
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the report was downloaded
    /// </summary>
    public DateTime? DownloadedAt { get; set; }

    /// <summary>
    /// Number of times downloaded
    /// </summary>
    public int DownloadCount { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
}