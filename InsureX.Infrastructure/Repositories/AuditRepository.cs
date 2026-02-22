using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;

namespace InsureX.Infrastructure.Repositories;

public class AuditRepository : Repository<AuditLog>, IAuditRepository
{
    public AuditRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<AuditLog>> GetRecentAlertsAsync(int count)
    {
        return await _context.AuditLogs
            .Where(a => a.Action.Contains("Alert") || a.Severity != null)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetRecentActivitiesAsync(int count)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, int entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserAsync(string userId, int count)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}