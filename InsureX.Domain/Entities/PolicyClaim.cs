namespace InsureX.Domain.Entities;

public class PolicyClaim : BaseEntity
{
    public Guid PolicyId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime DateOfLoss { get; set; }
    public DateTime DateReported { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime? StatusDate { get; set; }
    public string? AdjusterNotes { get; set; }
    
    // Navigation
    public virtual Policy Policy { get; set; }
}

public enum ClaimStatus
{
    Reported = 1,
    InReview = 2,
    Approved = 3,
    Paid = 4,
    Denied = 5,
    Closed = 6
}