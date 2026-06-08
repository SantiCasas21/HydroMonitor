using WaterDataService.Models;

namespace WaterDataService.Repositories;

public interface ISensorRepository
{
    Task<IEnumerable<Sensor>> GetAllAsync(bool? isActive = null, int page = 1, int pageSize = 20);
    Task<Sensor?> GetByIdAsync(Guid id);
    Task<Sensor> CreateAsync(Sensor sensor);
    Task<Sensor> UpdateAsync(Sensor sensor);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetTotalCountAsync(bool? isActive = null);
}
