using Microsoft.AspNetCore.Mvc;
using WaterDataService.Models.DTOs;
using WaterDataService.Services;

namespace WaterDataService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public SensorsController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    /// <summary>
    /// Get all sensors with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (sensors, totalCount) = await _sensorService.GetAllAsync(isActive, page, pageSize);
        return Ok(new { data = sensors, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    /// <summary>
    /// Get a sensor by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sensor = await _sensorService.GetByIdAsync(id);
        return sensor == null ? NotFound() : Ok(sensor);
    }

    /// <summary>
    /// Register a new sensor
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSensorDto dto)
    {
        var sensor = await _sensorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = sensor.Id }, sensor);
    }

    /// <summary>
    /// Update an existing sensor
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSensorDto dto)
    {
        var sensor = await _sensorService.UpdateAsync(id, dto);
        return sensor == null ? NotFound() : Ok(sensor);
    }

    /// <summary>
    /// Soft-delete a sensor
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sensorService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
