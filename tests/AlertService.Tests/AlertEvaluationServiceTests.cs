using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AlertService.Data;
using AlertService.Models;
using AlertService.Repositories;
using AlertService.Services;

namespace AlertService.Tests;

public class AlertEvaluationServiceTests
{
    private readonly AlertDbContext _context;
    private readonly AlertRuleRepository _ruleRepo;
    private readonly AlertRepository _alertRepo;
    private readonly AlertEvaluationService _service;

    public AlertEvaluationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AlertDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AlertDbContext(options);
        _ruleRepo = new AlertRuleRepository(_context);
        _alertRepo = new AlertRepository(_context);
        var logger = new Mock<ILogger<AlertEvaluationService>>().Object;
        _service = new AlertEvaluationService(_ruleRepo, _alertRepo, logger);
    }

    [Fact]
    public async Task EvaluateReadingAsync_ShouldCreateAlert_WhenThresholdExceeded()
    {
        // Arrange - Create a rule: pH max = 8.5
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            ParameterName = "pH",
            MaxThreshold = 8.5m,
            Severity = "Warning",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _ruleRepo.CreateAsync(rule);

        var reading = new SensorReadingData(
            Guid.NewGuid(), Guid.NewGuid(), "Test Sensor", "Test Location",
            Ph: 9.5m, Turbidity: 3.0m, DissolvedOxygen: 7.0m,
            Temperature: 22m, Conductivity: 400m
        );

        // Act
        var alerts = (await _service.EvaluateReadingAsync(reading)).ToList();

        // Assert
        alerts.Should().HaveCount(1);
        alerts[0].ParameterName.Should().Be("pH");
        alerts[0].ActualValue.Should().Be(9.5m);
        alerts[0].Severity.Should().Be("Warning");
        alerts[0].Message.Should().Contain("exceeded maximum");
    }

    [Fact]
    public async Task EvaluateReadingAsync_ShouldCreateAlert_WhenBelowMinimum()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            ParameterName = "DissolvedOxygen",
            MinThreshold = 4.0m,
            Severity = "Critical",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _ruleRepo.CreateAsync(rule);

        var reading = new SensorReadingData(
            Guid.NewGuid(), Guid.NewGuid(), "Sensor", "Loc",
            Ph: 7.2m, Turbidity: 3m, DissolvedOxygen: 2.5m,
            Temperature: 22m, Conductivity: 400m
        );

        // Act
        var alerts = (await _service.EvaluateReadingAsync(reading)).ToList();

        // Assert
        alerts.Should().HaveCount(1);
        alerts[0].ParameterName.Should().Be("DissolvedOxygen");
        alerts[0].Severity.Should().Be("Critical");
        alerts[0].Message.Should().Contain("fell below minimum");
    }

    [Fact]
    public async Task EvaluateReadingAsync_ShouldNotCreateAlert_WhenWithinThresholds()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            ParameterName = "Temperature",
            MaxThreshold = 30m,
            MinThreshold = 10m,
            Severity = "Warning",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _ruleRepo.CreateAsync(rule);

        var reading = new SensorReadingData(
            Guid.NewGuid(), Guid.NewGuid(), "Sensor", "Loc",
            Ph: 7.2m, Turbidity: 3m, DissolvedOxygen: 7m,
            Temperature: 22m, Conductivity: 400m
        );

        // Act
        var alerts = (await _service.EvaluateReadingAsync(reading)).ToList();

        // Assert
        alerts.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateReadingAsync_ShouldNotTrigger_WhenRuleIsInactive()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            ParameterName = "pH",
            MaxThreshold = 8.5m,
            Severity = "Warning",
            IsActive = false, // Inactive
            CreatedAt = DateTime.UtcNow
        };
        await _ruleRepo.CreateAsync(rule);

        var reading = new SensorReadingData(
            Guid.NewGuid(), Guid.NewGuid(), "Sensor", "Loc",
            Ph: 9.5m, Turbidity: 3m, DissolvedOxygen: 7m,
            Temperature: 22m, Conductivity: 400m
        );

        // Act
        var alerts = (await _service.EvaluateReadingAsync(reading)).ToList();

        // Assert
        alerts.Should().BeEmpty();
    }
}
