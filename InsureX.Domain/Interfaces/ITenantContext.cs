namespace InsureX.Domain.Interfaces;

public interface ITenantContext
{
    // Tenant Information - using int
    int? TenantId { get; }
    void SetTenantId(int tenantId);
    int? GetCurrentTenantId();
    bool HasValidTenant();
    
    // User Information
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
    string[] GetUserRoles();
    bool IsInRole(string role);
    
    // Additional useful methods for auditing
    DateTime GetCurrentRequestTime();
    string? GetCurrentUserFullName();
    string? GetClientIpAddress();
    string? GetCorrelationId();
}