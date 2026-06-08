using Microsoft.EntityFrameworkCore;
using WaterDataService.Data;
using WaterDataService.Models;

namespace WaterDataService.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly WaterDataDbContext _context;

    public SensorRepository(WaterDataDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sensor>> GetAllAsync(bool? isActive = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Sensors.Include(s => s.Readings.OrderByDescending(r => r.Timestamp).Take(1)).AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Sensor?> GetByIdAsync(Guid id)
    {
        return await _context.Sensors
            .Include(s => s.Readings.OrderByDescending(r => r.Timestamp).Take(1))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sensor> CreateAsync(Sensor sensor)
    {
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        return sensor;
    }

    public async Task<Sensor> UpdateAsync(Sensor sensor)
    {
        sensor.UpdatedAt = DateTime.UtcNow;
        _context.Sensors.Update(sensor);
        await _context.SaveChangesAsync();
        return sensor;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor == null) return false;

        sensor.IsActive = false;
        sensor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalCountAsync(bool? isActive = null)
    {
        var query = _context.Sensors.AsQueryable();
        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);
        return await query.CountAsync();
    }
}
