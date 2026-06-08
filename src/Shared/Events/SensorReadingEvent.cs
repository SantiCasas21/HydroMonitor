namespace Shared.Events;

public record SensorReadingEvent(
    Guid ReadingId,
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
