using Microsoft.AspNetCore.Mvc;
using AlertService.Models.DTOs;
using AlertService.Repositories;

namespace AlertService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(IAlertRepository alertRepository, ILogger<AlertsController> logger)
    {
        _alertRepository = alertRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? severity = null,
        [FromQuery] bool? isAcknowledged = null,
        [FromQuery] string? parameter = null)
    {
        var (alerts, totalCount) = await _alertRepository.GetAllAsync(page, pageSize, severity, isAcknowledged, parameter);
        var dtos = alerts.Select(MapToDto);
        return Ok(new { data = dtos, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var alert = await _alertRepository.GetByIdAsync(id);
        return alert == null ? NotFound() : Ok(MapToDto(alert));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] string? severity = null)
    {
        var alerts = await _alertRepository.GetActiveAlertsAsync(severity);
        return Ok(alerts.Select(MapToDto));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
        var toDate = to ?? DateTime.UtcNow;

        var stats = new AlertStatsDto(
            TotalAlerts: await _alertRepository.GetTotalCountAsync(fromDate, toDate),
            ActiveAlerts: await _alertRepository.GetTotalCountAsync(fromDate, toDate),
            CriticalAlerts: await _alertRepository.GetCountBySeverityAsync("Critical", fromDate, toDate),
            WarningAlerts: await _alertRepository.GetCountBySeverityAsync("Warning", fromDate, toDate),
            InfoAlerts: await _alertRepository.GetCountBySeverityAsync("Info", fromDate, toDate),
            AcknowledgedAlerts: 0 // Simplified
        );

        return Ok(stats);
    }

    [HttpPut("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, [FromBody] AcknowledgeRequest request)
    {
        var alert = await _alertRepository.GetByIdAsync(id);
        if (alert == null) return NotFound();

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedBy = request.AcknowledgedBy;

        var updated = await _alertRepository.UpdateAsync(alert);
        return Ok(MapToDto(updated));
    }

    private static AlertDto MapToDto(Models.Alert alert) => new(
        alert.Id, alert.AlertRuleId, alert.AlertRule?.Description,
        alert.SensorId, alert.ReadingId, alert.ParameterName, alert.ActualValue,
        alert.MinThreshold, alert.MaxThreshold, alert.Message, alert.Severity,
        alert.IsAcknowledged, alert.AcknowledgedAt, alert.AcknowledgedBy, alert.CreatedAt
    );
}
