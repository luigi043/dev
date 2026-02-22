namespace InsureX.Domain.Entities;

public class PolicyDocument : BaseEntity
{
    public Guid PolicyId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Application, Certificate, Endorsement, etc.
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    
    // Navigation
    public virtual Policy Policy { get; set; }
}