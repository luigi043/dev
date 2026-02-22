using System;

namespace InsureX.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public string? DatabaseConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Settings { get; set; }
    public DateTime? SubscriptionExpiry { get; set; }
}