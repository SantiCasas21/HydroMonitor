namespace WaterDataService.Models.DTOs;

public record SensorReadingDto(
    Guid Id,
    Guid SensorId,
    string SensorName,
    string Location,
    decimal Ph,
    decimal Turbidity,
    decimal DissolvedOxygen,
    decimal Temperature,
    decimal Conductivity,
    DateTime Timestamp
);

public record CreateReadingDto(
    Guid SensorId,
    decimal Ph,
    decimal Turbidity,
    decimal DissolvedOxygen,
    decimal Temperature,
    decimal Conductivity
);

public record ReadingStatsDto(
    decimal AvgPh,
    decimal AvgTurbidity,
    decimal AvgDissolvedOxygen,
    decimal AvgTemperature,
    decimal AvgConductivity,
    decimal MinPh,
    decimal MaxPh,
    decimal MinTurbidity,
    decimal MaxTurbidity,
    int TotalReadings,
    DateTime From,
    DateTime To
);

public record HistoricalQueryDto(
    Guid SensorId,
    string Parameter,
    DateTime From,
    DateTime To,
    string? Interval = null
);

public record ParameterDataPointDto(
    DateTime Timestamp,
    decimal Value
);
