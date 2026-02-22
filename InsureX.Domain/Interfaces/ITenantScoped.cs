using System;

namespace InsureX.Domain.Interfaces
{
    /// <summary>
    /// Interface for entities that are scoped to a specific tenant in a multi-tenant architecture.
    /// This ensures data isolation between different tenants.
    /// </summary>
    public interface ITenantScoped
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tenant that owns this entity.
        /// This ID is used to isolate data between different tenants and enforce
        /// multi-tenancy security boundaries.
        /// </summary>
        Guid TenantId { get; set; }
    }
}