using Microsoft.AspNetCore.Mvc;
using AlertService.Models.DTOs;
using AlertService.Services;

namespace AlertService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleService _ruleService;

    public AlertRulesController(IAlertRuleService ruleService) => _ruleService = ruleService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null, [FromQuery] string? parameter = null)
    {
        var rules = await _ruleService.GetAllAsync(isActive, parameter);
        return Ok(rules);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rule = await _ruleService.GetByIdAsync(id);
        return rule == null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertRuleDto dto)
    {
        var rule = await _ruleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAlertRuleDto dto)
    {
        var rule = await _ruleService.UpdateAsync(id, dto);
        return rule == null ? NotFound() : Ok(rule);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _ruleService.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
