using System;

namespace InsureX.Domain.Interfaces;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}