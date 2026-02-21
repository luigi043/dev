using System;
using InsureX.Domain.Entities;

namespace InsureX.Domain.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int? TenantId { get; set; }
        public string? TenantCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}