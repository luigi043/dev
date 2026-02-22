using InsureX.Domain.Entities;

namespace InsureX.Domain.Interfaces;

public interface IAuditRepository : IRepository<AuditLog>
{
    // Audit specific methods with int IDs
    Task<List<AuditLog>> GetRecentAlertsAsync(int tenantId, int count);
    Task<List<AuditLog>> GetRecentActivitiesAsync(int tenantId, int count);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId);
    Task<List<AuditLog>> GetByUserAsync(string userId, int tenantId, int count);
    Task<List<AuditLog>> GetByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<List<AuditLog>> GetByActionTypeAsync(string actionType, int tenantId, int days = 7);
    Task<List<AuditLog>> GetByTenantIdAsync(int tenantId, int page, int pageSize);
    
    // Statistics
    Task<Dictionary<string, int>> GetAuditStatisticsAsync(int tenantId, int days = 30);
    Task<List<AuditLog>> GetUserActivityAsync(string userId, int tenantId, int days = 7);
    
    // Cleanup
    Task<int> DeleteOldLogsAsync(int daysOld);
}