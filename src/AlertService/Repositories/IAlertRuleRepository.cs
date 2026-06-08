using AlertService.Models;

namespace AlertService.Repositories;

public interface IAlertRuleRepository
{
    Task<IEnumerable<AlertRule>> GetAllAsync(bool? isActive = null, string? parameter = null);
    Task<AlertRule?> GetByIdAsync(Guid id);
    Task<AlertRule> CreateAsync(AlertRule rule);
    Task<AlertRule> UpdateAsync(AlertRule rule);
    Task<bool> DeleteAsync(Guid id);
}
