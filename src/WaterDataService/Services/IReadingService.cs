using WaterDataService.Models.DTOs;

namespace WaterDataService.Services;

public interface IReadingService
{
    Task<SensorReadingDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<SensorReadingDto> Readings, int TotalCount)> GetBySensorIdAsync(Guid sensorId, int page = 1, int pageSize = 50);
    Task<SensorReadingDto> CreateAsync(CreateReadingDto dto);
    Task<IEnumerable<SensorReadingDto>> GetLatestForAllSensorsAsync(Guid? sensorId = null);
    Task<IEnumerable<ParameterDataPointDto>> GetHistoryAsync(HistoricalQueryDto query);
    Task<ReadingStatsDto> GetStatsAsync(Guid sensorId, DateTime? from = null, DateTime? to = null);
}
