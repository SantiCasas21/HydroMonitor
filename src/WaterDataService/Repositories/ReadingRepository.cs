using Microsoft.EntityFrameworkCore;
using WaterDataService.Data;
using WaterDataService.Models;

namespace WaterDataService.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly WaterDataDbContext _context;

    public ReadingRepository(WaterDataDbContext context)
    {
        _context = context;
    }

    public async Task<SensorReading?> GetByIdAsync(Guid id)
    {
        return await _context.SensorReadings
            .Include(r => r.Sensor)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<SensorReading>> GetBySensorIdAsync(Guid sensorId, int page = 1, int pageSize = 50)
    {
        return await _context.SensorReadings
            .Include(r => r.Sensor)
            .Where(r => r.SensorId == sensorId)
            .OrderByDescending(r => r.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SensorReading> CreateAsync(SensorReading reading)
    {
        _context.SensorReadings.Add(reading);
        await _context.SaveChangesAsync();
        return reading;
    }

    public async Task<IEnumerable<SensorReading>> GetLatestForAllSensorsAsync(Guid? sensorId = null)
    {
        var latestTimestamps = _context.SensorReadings
            .GroupBy(r => r.SensorId)
            .Select(g => new { SensorId = g.Key, MaxTimestamp = g.Max(r => r.Timestamp) });

        var query = from r in _context.SensorReadings.Include(r => r.Sensor)
                    join lt in latestTimestamps on new { r.SensorId, r.Timestamp } equals new { lt.SensorId, Timestamp = lt.MaxTimestamp }
                    select r;

        if (sensorId.HasValue)
            query = query.Where(r => r.SensorId == sensorId.Value);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<SensorReading>> GetHistoryAsync(Guid sensorId, string parameter, DateTime from, DateTime to)
    {
        var query = _context.SensorReadings
            .Where(r => r.SensorId == sensorId && r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp);

        return await query.Take(5000).ToListAsync();
    }

    public async Task<int> GetCountBySensorIdAsync(Guid sensorId)
    {
        return await _context.SensorReadings.CountAsync(r => r.SensorId == sensorId);
    }
}
