namespace InsureX.Domain.Interfaces;

/// <summary>
/// Interface for entities that are scoped to a specific tenant
/// </summary>
public interface ITenantScoped
{
    /// <summary>
    /// Tenant identifier - using int for consistency with SQL Server
    /// </summary>
    int TenantId { get; set; }
}