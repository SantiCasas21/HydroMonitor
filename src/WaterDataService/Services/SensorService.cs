using WaterDataService.Models;
using WaterDataService.Models.DTOs;
using WaterDataService.Repositories;

namespace WaterDataService.Services;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;

    public SensorService(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    public async Task<(IEnumerable<SensorDto> Sensors, int TotalCount)> GetAllAsync(bool? isActive = null, int page = 1, int pageSize = 20)
    {
        var sensors = await _sensorRepository.GetAllAsync(isActive, page, pageSize);
        var totalCount = await _sensorRepository.GetTotalCountAsync(isActive);

        var dtos = sensors.Select(MapToDto);
        return (dtos, totalCount);
    }

    public async Task<SensorDto?> GetByIdAsync(Guid id)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id);
        return sensor == null ? null : MapToDto(sensor);
    }

    public async Task<SensorDto> CreateAsync(CreateSensorDto dto)
    {
        var sensor = new Sensor
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Location = dto.Location,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Description = dto.Description,
            IsActive = true,
            InstalledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _sensorRepository.CreateAsync(sensor);
        return MapToDto(created);
    }

    public async Task<SensorDto?> UpdateAsync(Guid id, UpdateSensorDto dto)
    {
        var sensor = await _sensorRepository.GetByIdAsync(id);
        if (sensor == null) return null;

        if (dto.Name != null) sensor.Name = dto.Name;
        if (dto.Location != null) sensor.Location = dto.Location;
        if (dto.Latitude != null) sensor.Latitude = dto.Latitude;
        if (dto.Longitude != null) sensor.Longitude = dto.Longitude;
        if (dto.Description != null) sensor.Description = dto.Description;
        if (dto.IsActive.HasValue) sensor.IsActive = dto.IsActive.Value;

        var updated = await _sensorRepository.UpdateAsync(sensor);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id) => await _sensorRepository.DeleteAsync(id);

    private static SensorDto MapToDto(Sensor sensor)
    {
        var latestReading = sensor.Readings?.FirstOrDefault();
        var readingDto = latestReading != null ? new SensorReadingDto(
            latestReading.Id, latestReading.SensorId, sensor.Name, sensor.Location,
            latestReading.Ph, latestReading.Turbidity, latestReading.DissolvedOxygen,
            latestReading.Temperature, latestReading.Conductivity, latestReading.Timestamp
        ) : null;

        return new SensorDto(
            sensor.Id, sensor.Name, sensor.Location, sensor.Latitude, sensor.Longitude,
            sensor.Description, sensor.IsActive, sensor.InstalledAt, readingDto
        );
    }
}
