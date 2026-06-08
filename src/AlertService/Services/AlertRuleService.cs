using AlertService.Models;
using AlertService.Models.DTOs;
using AlertService.Repositories;

namespace AlertService.Services;

public class AlertRuleService : IAlertRuleService
{
    private readonly IAlertRuleRepository _ruleRepository;

    public AlertRuleService(IAlertRuleRepository ruleRepository) => _ruleRepository = ruleRepository;

    public async Task<IEnumerable<AlertRuleDto>> GetAllAsync(bool? isActive = null, string? parameter = null)
    {
        var rules = await _ruleRepository.GetAllAsync(isActive, parameter);
        return rules.Select(MapToDto);
    }

    public async Task<AlertRuleDto?> GetByIdAsync(Guid id)
    {
        var rule = await _ruleRepository.GetByIdAsync(id);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<AlertRuleDto> CreateAsync(CreateAlertRuleDto dto)
    {
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            ParameterName = dto.ParameterName,
            MinThreshold = dto.MinThreshold,
            MaxThreshold = dto.MaxThreshold,
            Severity = dto.Severity,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _ruleRepository.CreateAsync(rule);
        return MapToDto(created);
    }

    public async Task<AlertRuleDto?> UpdateAsync(Guid id, UpdateAlertRuleDto dto)
    {
        var rule = await _ruleRepository.GetByIdAsync(id);
        if (rule == null) return null;

        if (dto.ParameterName != null) rule.ParameterName = dto.ParameterName;
        if (dto.MinThreshold != null) rule.MinThreshold = dto.MinThreshold;
        if (dto.MaxThreshold != null) rule.MaxThreshold = dto.MaxThreshold;
        if (dto.Severity != null) rule.Severity = dto.Severity;
        if (dto.IsActive.HasValue) rule.IsActive = dto.IsActive.Value;
        if (dto.Description != null) rule.Description = dto.Description;

        var updated = await _ruleRepository.UpdateAsync(rule);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id) => await _ruleRepository.DeleteAsync(id);

    private static AlertRuleDto MapToDto(AlertRule rule) => new(
        rule.Id, rule.ParameterName, rule.MinThreshold, rule.MaxThreshold,
        rule.Severity, rule.IsActive, rule.Description, rule.CreatedAt, rule.UpdatedAt
    );
}
