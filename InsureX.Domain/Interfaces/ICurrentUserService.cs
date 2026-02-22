namespace InsureX.Domain.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    string? GetCurrentUserName();
    bool IsAuthenticated();
    bool IsInRole(string role);
    string[] GetRoles();
    bool HasPermission(string permission);
}