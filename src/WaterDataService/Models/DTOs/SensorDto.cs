namespace WaterDataService.Models.DTOs;

public record SensorDto(
    Guid Id,
    string Name,
    string Location,
    decimal? Latitude,
    decimal? Longitude,
    string? Description,
    bool IsActive,
    DateTime InstalledAt,
    SensorReadingDto? LatestReading
);

public record CreateSensorDto(
    string Name,
    string Location,
    decimal? Latitude,
    decimal? Longitude,
    string? Description
);

public record UpdateSensorDto(
    string? Name,
    string? Location,
    decimal? Latitude,
    decimal? Longitude,
    string? Description,
    bool? IsActive
);
