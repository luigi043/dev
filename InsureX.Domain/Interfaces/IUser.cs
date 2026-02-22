namespace InsureX.Domain.Interfaces;

public interface IUser
{
    string Id { get; } // Identity uses string for UserId
    string UserName { get; }
    string Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    string FullName => $"{FirstName} {LastName}".Trim();
    int? TenantId { get; } // This is our int tenant ID
    bool IsActive { get; }
    DateTime? LastLoginAt { get; }
}