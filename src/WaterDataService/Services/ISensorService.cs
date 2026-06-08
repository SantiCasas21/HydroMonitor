using WaterDataService.Models.DTOs;

namespace WaterDataService.Services;

public interface ISensorService
{
    Task<(IEnumerable<SensorDto> Sensors, int TotalCount)> GetAllAsync(bool? isActive = null, int page = 1, int pageSize = 20);
    Task<SensorDto?> GetByIdAsync(Guid id);
    Task<SensorDto> CreateAsync(CreateSensorDto dto);
    Task<SensorDto?> UpdateAsync(Guid id, UpdateSensorDto dto);
    Task<bool> DeleteAsync(Guid id);
}
