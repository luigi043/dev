using System.Collections.Generic;

namespace InsureX.Application.Interfaces;

public interface ICurrentUserService
{
    // Properties
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    
    // Methods (for backward compatibility)
    string? GetCurrentUserId() => UserId;
    string? GetCurrentUserEmail() => Email;
    string? GetCurrentUserName() => UserName;
    bool IsInRole(string role);
    string? GetClaimValue(string claimType);
}