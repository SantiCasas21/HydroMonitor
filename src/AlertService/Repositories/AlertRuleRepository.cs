using Microsoft.EntityFrameworkCore;
using AlertService.Data;
using AlertService.Models;

namespace AlertService.Repositories;

public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly AlertDbContext _context;

    public AlertRuleRepository(AlertDbContext context) => _context = context;

    public async Task<IEnumerable<AlertRule>> GetAllAsync(bool? isActive = null, string? parameter = null)
    {
        var query = _context.AlertRules.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(parameter))
            query = query.Where(r => r.ParameterName == parameter);

        return await query.OrderBy(r => r.ParameterName).ThenBy(r => r.Severity).ToListAsync();
    }

    public async Task<AlertRule?> GetByIdAsync(Guid id) =>
        await _context.AlertRules.FindAsync(id);

    public async Task<AlertRule> CreateAsync(AlertRule rule)
    {
        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<AlertRule> UpdateAsync(AlertRule rule)
    {
        rule.UpdatedAt = DateTime.UtcNow;
        _context.AlertRules.Update(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rule = await _context.AlertRules.FindAsync(id);
        if (rule == null) return false;

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
