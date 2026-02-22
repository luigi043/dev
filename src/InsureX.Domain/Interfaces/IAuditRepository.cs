using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IAuditRepository : IRepository<AuditLog>
{
    Task<List<AuditLog>> GetRecentAlertsAsync(int count);
    Task<List<AuditLog>> GetRecentActivitiesAsync(int count);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    Task<List<AuditLog>> GetByUserAsync(string userId, int count);
}