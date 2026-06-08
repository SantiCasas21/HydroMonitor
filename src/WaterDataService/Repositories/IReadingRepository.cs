using WaterDataService.Models;

namespace WaterDataService.Repositories;

public interface IReadingRepository
{
    Task<SensorReading?> GetByIdAsync(Guid id);
    Task<IEnumerable<SensorReading>> GetBySensorIdAsync(Guid sensorId, int page = 1, int pageSize = 50);
    Task<SensorReading> CreateAsync(SensorReading reading);
    Task<IEnumerable<SensorReading>> GetLatestForAllSensorsAsync(Guid? sensorId = null);
    Task<IEnumerable<SensorReading>> GetHistoryAsync(Guid sensorId, string parameter, DateTime from, DateTime to);
    Task<int> GetCountBySensorIdAsync(Guid sensorId);
}
