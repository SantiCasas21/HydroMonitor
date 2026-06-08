using Shared.Constants;
using WaterDataService.Models;
using WaterDataService.Models.DTOs;
using WaterDataService.Repositories;

namespace WaterDataService.Services;

public class ReadingService : IReadingService
{
    private readonly IReadingRepository _readingRepository;
    private readonly ISensorRepository _sensorRepository;

    public ReadingService(IReadingRepository readingRepository, ISensorRepository sensorRepository)
    {
        _readingRepository = readingRepository;
        _sensorRepository = sensorRepository;
    }

    public async Task<SensorReadingDto?> GetByIdAsync(Guid id)
    {
        var reading = await _readingRepository.GetByIdAsync(id);
        return reading == null ? null : MapToDto(reading);
    }

    public async Task<(IEnumerable<SensorReadingDto> Readings, int TotalCount)> GetBySensorIdAsync(Guid sensorId, int page = 1, int pageSize = 50)
    {
        var readings = await _readingRepository.GetBySensorIdAsync(sensorId, page, pageSize);
        var totalCount = await _readingRepository.GetCountBySensorIdAsync(sensorId);
        return (readings.Select(MapToDto), totalCount);
    }

    public async Task<SensorReadingDto> CreateAsync(CreateReadingDto dto)
    {
        var reading = new SensorReading
        {
            Id = Guid.NewGuid(),
            SensorId = dto.SensorId,
            Ph = dto.Ph,
            Turbidity = dto.Turbidity,
            DissolvedOxygen = dto.DissolvedOxygen,
            Temperature = dto.Temperature,
            Conductivity = dto.Conductivity,
            Timestamp = DateTime.UtcNow
        };

        var created = await _readingRepository.CreateAsync(reading);

        // Reload with sensor info
        var withSensor = await _readingRepository.GetByIdAsync(created.Id);
        return MapToDto(withSensor!);
    }

    public async Task<IEnumerable<SensorReadingDto>> GetLatestForAllSensorsAsync(Guid? sensorId = null)
    {
        var readings = await _readingRepository.GetLatestForAllSensorsAsync(sensorId);
        return readings.Select(MapToDto);
    }

    public async Task<IEnumerable<ParameterDataPointDto>> GetHistoryAsync(HistoricalQueryDto query)
    {
        var readings = await _readingRepository.GetHistoryAsync(query.SensorId, query.Parameter, query.From, query.To);

        return query.Parameter.ToLower() switch
        {
            "ph" => readings.Select(r => new ParameterDataPointDto(r.Timestamp, r.Ph)),
            "turbidity" => readings.Select(r => new ParameterDataPointDto(r.Timestamp, r.Turbidity)),
            "dissolvedoxygen" => readings.Select(r => new ParameterDataPointDto(r.Timestamp, r.DissolvedOxygen)),
            "temperature" => readings.Select(r => new ParameterDataPointDto(r.Timestamp, r.Temperature)),
            "conductivity" => readings.Select(r => new ParameterDataPointDto(r.Timestamp, r.Conductivity)),
            _ => Enumerable.Empty<ParameterDataPointDto>()
        };
    }

    public async Task<ReadingStatsDto> GetStatsAsync(Guid sensorId, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
        var toDate = to ?? DateTime.UtcNow;
        var readings = (await _readingRepository.GetHistoryAsync(sensorId, "all", fromDate, toDate)).ToList();

        if (!readings.Any())
            return new ReadingStatsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, fromDate, toDate);

        return new ReadingStatsDto(
            AvgPh: Math.Round((decimal)readings.Average(r => (double)r.Ph), 2),
            AvgTurbidity: Math.Round((decimal)readings.Average(r => (double)r.Turbidity), 2),
            AvgDissolvedOxygen: Math.Round((decimal)readings.Average(r => (double)r.DissolvedOxygen), 2),
            AvgTemperature: Math.Round((decimal)readings.Average(r => (double)r.Temperature), 2),
            AvgConductivity: Math.Round((decimal)readings.Average(r => (double)r.Conductivity), 2),
            MinPh: readings.Min(r => r.Ph),
            MaxPh: readings.Max(r => r.Ph),
            MinTurbidity: readings.Min(r => r.Turbidity),
            MaxTurbidity: readings.Max(r => r.Turbidity),
            TotalReadings: readings.Count,
            From: fromDate,
            To: toDate
        );
    }

    private static SensorReadingDto MapToDto(SensorReading reading)
    {
        return new SensorReadingDto(
            reading.Id,
            reading.SensorId,
            reading.Sensor?.Name ?? "Unknown",
            reading.Sensor?.Location ?? "Unknown",
            reading.Ph,
            reading.Turbidity,
            reading.DissolvedOxygen,
            reading.Temperature,
            reading.Conductivity,
            reading.Timestamp
        );
    }
}
