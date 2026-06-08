using Microsoft.AspNetCore.Mvc;
using WaterDataService.Models.DTOs;
using WaterDataService.Services;

namespace WaterDataService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly IReadingService _readingService;

    public ReadingsController(IReadingService readingService)
    {
        _readingService = readingService;
    }

    /// <summary>
    /// Get readings for a sensor (paginated)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBySensor([FromQuery] Guid sensorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var (readings, totalCount) = await _readingService.GetBySensorIdAsync(sensorId, page, pageSize);
        return Ok(new { data = readings, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    /// <summary>
    /// Get a single reading by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var reading = await _readingService.GetByIdAsync(id);
        return reading == null ? NotFound() : Ok(reading);
    }

    /// <summary>
    /// Get the latest reading for each active sensor
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] Guid? sensorId = null)
    {
        var readings = await _readingService.GetLatestForAllSensorsAsync(sensorId);
        return Ok(readings);
    }

    /// <summary>
    /// Get historical time-series data for a specific parameter
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid sensorId, [FromQuery] string parameter, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var query = new HistoricalQueryDto(sensorId, parameter, from, to);
        var data = await _readingService.GetHistoryAsync(query);
        return Ok(data);
    }

    /// <summary>
    /// Get aggregated statistics for a sensor
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] Guid sensorId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var stats = await _readingService.GetStatsAsync(sensorId, from, to);
        return Ok(stats);
    }

    /// <summary>
    /// Ingest a new sensor reading
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReadingDto dto)
    {
        var reading = await _readingService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = reading.Id }, reading);
    }
}
