namespace AlertService.Models.DTOs;

public record AlertDto(
    Guid Id,
    Guid? AlertRuleId,
    string? RuleDescription,
    Guid SensorId,
    Guid ReadingId,
    string ParameterName,
    decimal ActualValue,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string Message,
    string Severity,
    bool IsAcknowledged,
    DateTime? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTime CreatedAt
);

public record AcknowledgeRequest(string AcknowledgedBy);

public record AlertStatsDto(
    int TotalAlerts,
    int ActiveAlerts,
    int CriticalAlerts,
    int WarningAlerts,
    int InfoAlerts,
    int AcknowledgedAlerts
);
