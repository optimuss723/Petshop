using Microsoft.EntityFrameworkCore;
using Order.Service.Data;
using Order.Service.DTOs;
using Order.Service.Entities;

namespace Order.Service.Services;

// ── INTERFACE ────────────────────────────────────────────────────

public interface IAuditLogService
{
    Task LogAsync(int staffId, string staffName, string action, int? orderId, string? details);
    Task<List<AuditLogResponse>> GetByStaffAsync(int staffId, int page = 1, int pageSize = 30);
    Task<List<AuditLogResponse>> GetAllAsync(int page = 1, int pageSize = 50);
}

// ── IMPLEMENTATION ───────────────────────────────────────────────

public class AuditLogService : IAuditLogService
{
    private readonly OrderDbContext _db;
    public AuditLogService(OrderDbContext db) => _db = db;

    public async Task LogAsync(int staffId, string staffName, string action, int? orderId, string? details)
    {
        _db.StaffAuditLogs.Add(new StaffAuditLog
        {
            StaffId   = staffId,
            StaffName = staffName,
            Action    = action,
            OrderId   = orderId,
            Details   = details,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditLogResponse>> GetByStaffAsync(int staffId, int page = 1, int pageSize = 30)
    {
        return await _db.StaffAuditLogs
            .Where(l => l.StaffId == staffId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => ToDto(l))
            .ToListAsync();
    }

    public async Task<List<AuditLogResponse>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        return await _db.StaffAuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => ToDto(l))
            .ToListAsync();
    }

    private static AuditLogResponse ToDto(StaffAuditLog l) => new(
        l.Id, l.StaffId, l.StaffName, l.Action, l.OrderId, l.Details, l.CreatedAt);
}
