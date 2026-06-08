using Microsoft.EntityFrameworkCore;
using AlertService.Data;
using AlertService.Models;

namespace AlertService.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AlertDbContext _context;

    public AlertRepository(AlertDbContext context) => _context = context;

    public async Task<(IEnumerable<Alert> Alerts, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? severity = null, bool? isAcknowledged = null, string? parameter = null)
    {
        var query = _context.Alerts.Include(a => a.AlertRule).AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity == severity);

        if (isAcknowledged.HasValue)
            query = query.Where(a => a.IsAcknowledged == isAcknowledged.Value);

        if (!string.IsNullOrWhiteSpace(parameter))
            query = query.Where(a => a.ParameterName == parameter);

        var totalCount = await query.CountAsync();

        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (alerts, totalCount);
    }

    public async Task<Alert?> GetByIdAsync(Guid id) =>
        await _context.Alerts.Include(a => a.AlertRule).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Alert> CreateAsync(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<Alert> UpdateAsync(Alert alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(string? severity = null)
    {
        var query = _context.Alerts.Include(a => a.AlertRule).Where(a => !a.IsAcknowledged);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity == severity);

        return await query.OrderByDescending(a => a.CreatedAt).Take(50).ToListAsync();
    }

    public async Task<int> GetCountBySeverityAsync(string severity, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Alerts.Where(a => a.Severity == severity);
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetTotalCountAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Alerts.AsQueryable();
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(a => a.CreatedAt <= to.Value);
        return await query.CountAsync();
    }
}
