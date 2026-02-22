using System;

namespace InsureX.Domain.Interfaces;

public interface ITenantContext
{
    Guid? GetCurrentTenantId();
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
    string[] GetUserRoles();
}