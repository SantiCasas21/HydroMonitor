using AlertService.Models;
using AlertService.Models.DTOs;
using AlertService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AlertService.Services;

public interface IAlertEvaluationService
{
    Task<IEnumerable<AlertDto>> EvaluateReadingAsync(SensorReadingData reading);
}

public class AlertEvaluationService : IAlertEvaluationService
{
    private readonly IAlertRuleRepository _ruleRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AlertEvaluationService> _logger;

    public AlertEvaluationService(IAlertRuleRepository ruleRepository, IAlertRepository alertRepository, ILogger<AlertEvaluationService> logger)
    {
        _ruleRepository = ruleRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AlertDto>> EvaluateReadingAsync(SensorReadingData reading)
    {
        var rules = await _ruleRepository.GetAllAsync(isActive: true);
        var alerts = new List<Alert>();

        foreach (var rule in rules)
        {
            var actualValue = GetParameterValue(reading, rule.ParameterName);

            bool isBreach = false;
            string breachType = "";

            if (rule.MaxThreshold.HasValue && actualValue > rule.MaxThreshold.Value)
            {
                isBreach = true;
                breachType = $"exceeded maximum threshold of {rule.MaxThreshold}";
            }
            else if (rule.MinThreshold.HasValue && actualValue < rule.MinThreshold.Value)
            {
                isBreach = true;
                breachType = $"fell below minimum threshold of {rule.MinThreshold}";
            }

            if (isBreach)
            {
                var alert = new Alert
                {
                    Id = Guid.NewGuid(),
                    AlertRuleId = rule.Id,
                    SensorId = reading.SensorId,
                    ReadingId = reading.ReadingId,
                    ParameterName = rule.ParameterName,
                    ActualValue = actualValue,
                    MinThreshold = rule.MinThreshold,
                    MaxThreshold = rule.MaxThreshold,
                    Severity = rule.Severity,
                    Message = $"[{rule.Severity.ToUpper()}] {rule.ParameterName} = {actualValue} {breachType}. Sensor: {reading.SensorName} ({reading.Location})",
                    CreatedAt = DateTime.UtcNow
                };

                alert = await _alertRepository.CreateAsync(alert);
                // Reload with rule info
                alert = await _alertRepository.GetByIdAsync(alert.Id);
                alerts.Add(alert!);

                _logger.LogWarning("Alert triggered: {Message}", alert.Message);
            }
        }

        return alerts.Select(MapToDto);
    }

    private static decimal GetParameterValue(SensorReadingData reading, string parameter)
    {
        return parameter switch
        {
            "pH" => reading.Ph,
            "Turbidity" => reading.Turbidity,
            "DissolvedOxygen" => reading.DissolvedOxygen,
            "Temperature" => reading.Temperature,
            "Conductivity" => reading.Conductivity,
            _ => 0
        };
    }

    private static AlertDto MapToDto(Alert alert) => new(
        alert.Id,
        alert.AlertRuleId,
        alert.AlertRule?.Description,
        alert.SensorId,
        alert.ReadingId,
        alert.ParameterName,
        alert.ActualValue,
        alert.MinThreshold,
        alert.MaxThreshold,
        alert.Message,
        alert.Severity,
        alert.IsAcknowledged,
        alert.AcknowledgedAt,
        alert.AcknowledgedBy,
        alert.CreatedAt
    );
}

public record SensorReadingData(
    Guid ReadingId,
    Guid SensorId,
    string SensorName,
    string Location,
    decimal Ph,
    decimal Turbidity,
    decimal DissolvedOxygen,
    decimal Temperature,
    decimal Conductivity
);
