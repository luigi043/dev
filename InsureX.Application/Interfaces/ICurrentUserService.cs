namespace InsureX.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    Guid? TenantId { get; }
    
    // Method versions for backward compatibility
    string? GetCurrentUserId();
    string? GetCurrentUserName();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
}namespace InsureX.Application.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserName();
    bool IsAuthenticated();
    IEnumerable<string> GetUserRoles();
    bool IsInRole(string role);
}