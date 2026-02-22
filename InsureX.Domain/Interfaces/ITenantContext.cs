using System;
using System.Collections.Generic;

namespace InsureX.Domain.Interfaces;

public interface ITenantContext
{
    // Tenant Information
    Guid? TenantId { get; }
    void SetTenantId(Guid tenantId);
    Guid? GetCurrentTenantId();
    
    // User Information
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
    string[] GetUserRoles();
    
    // Additional useful methods for auditing
    DateTime GetCurrentRequestTime();
    string? GetCurrentUserFullName();
    string? GetClientIpAddress();
}