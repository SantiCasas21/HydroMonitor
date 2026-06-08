namespace AlertService.Models.DTOs;

public record AlertRuleDto(
    Guid Id,
    string ParameterName,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string Severity,
    bool IsActive,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAlertRuleDto(
    string ParameterName,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string Severity,
    string? Description
);

public record UpdateAlertRuleDto(
    string? ParameterName,
    decimal? MinThreshold,
    decimal? MaxThreshold,
    string? Severity,
    bool? IsActive,
    string? Description
);
