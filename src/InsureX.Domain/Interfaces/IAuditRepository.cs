using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
namespace InsureX.Application.Interfaces {
public interface IAuditRepository : IRepository<AuditLog>
{
    Task<List<AuditLog>> GetRecentAlertsAsync(int count);
    Task<List<AuditLog>> GetRecentActivitiesAsync(int count);
}
}
