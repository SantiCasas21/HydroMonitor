using AlertService.Models;

namespace AlertService.Repositories;

public interface IAlertRepository
{
    Task<(IEnumerable<Alert> Alerts, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 20, string? severity = null, bool? isAcknowledged = null, string? parameter = null);
    Task<Alert?> GetByIdAsync(Guid id);
    Task<Alert> CreateAsync(Alert alert);
    Task<Alert> UpdateAsync(Alert alert);
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(string? severity = null);
    Task<int> GetCountBySeverityAsync(string severity, DateTime? from = null, DateTime? to = null);
    Task<int> GetTotalCountAsync(DateTime? from = null, DateTime? to = null);
}
