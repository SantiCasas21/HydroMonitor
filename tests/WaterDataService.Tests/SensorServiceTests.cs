using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using WaterDataService.Data;
using WaterDataService.Models;
using WaterDataService.Models.DTOs;
using WaterDataService.Repositories;
using WaterDataService.Services;

namespace WaterDataService.Tests;

public class SensorServiceTests
{
    private readonly WaterDataDbContext _context;
    private readonly SensorRepository _repository;
    private readonly SensorService _service;

    public SensorServiceTests()
    {
        var options = new DbContextOptionsBuilder<WaterDataDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new WaterDataDbContext(options);
        _repository = new SensorRepository(_context);
        _service = new SensorService(_repository);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSensor()
    {
        // Arrange
        var dto = new CreateSensorDto("Test Sensor", "Test Location", null, null, "Test Description");

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Sensor");
        result.Location.Should().Be("Test Location");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSensor_WhenExists()
    {
        // Arrange
        var created = await _service.CreateAsync(new CreateSensorDto("Find Me", "Somewhere", null, null, null));

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSensorsWithPagination()
    {
        // Arrange
        await _service.CreateAsync(new CreateSensorDto("Sensor A", "Loc A", null, null, null));
        await _service.CreateAsync(new CreateSensorDto("Sensor B", "Loc B", null, null, null));
        await _service.CreateAsync(new CreateSensorDto("Sensor C", "Loc C", null, null, null));

        // Act
        var (sensors, totalCount) = await _service.GetAllAsync(page: 1, pageSize: 2);

        // Assert
        sensors.Should().HaveCount(2);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        // Arrange
        var created = await _service.CreateAsync(new CreateSensorDto("To Delete", "Loc", null, null, null));

        // Act
        var result = await _service.DeleteAsync(created.Id);
        var deleted = await _service.GetByIdAsync(created.Id);

        // Assert
        result.Should().BeTrue();
        deleted!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSensorFields()
    {
        // Arrange
        var created = await _service.CreateAsync(new CreateSensorDto("Original", "Loc", null, null, null));
        var updateDto = new UpdateSensorDto("Updated Name", null, null, null, null, null);

        // Act
        var updated = await _service.UpdateAsync(created.Id, updateDto);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Location.Should().Be("Loc"); // unchanged
    }
}
