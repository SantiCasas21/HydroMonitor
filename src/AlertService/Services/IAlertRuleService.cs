using AlertService.Models.DTOs;

namespace AlertService.Services;

public interface IAlertRuleService
{
    Task<IEnumerable<AlertRuleDto>> GetAllAsync(bool? isActive = null, string? parameter = null);
    Task<AlertRuleDto?> GetByIdAsync(Guid id);
    Task<AlertRuleDto> CreateAsync(CreateAlertRuleDto dto);
    Task<AlertRuleDto?> UpdateAsync(Guid id, UpdateAlertRuleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
