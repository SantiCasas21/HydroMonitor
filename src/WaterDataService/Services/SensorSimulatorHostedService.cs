using Microsoft.AspNetCore.SignalR;
using WaterDataService.Hubs;
using WaterDataService.Models;
using WaterDataService.Models.DTOs;
using WaterDataService.Repositories;

namespace WaterDataService.Services;

public class SensorSimulatorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<WaterDataHub> _hubContext;
    private readonly ILogger<SensorSimulatorHostedService> _logger;
    private readonly Random _random = new();
    private readonly List<Guid> _sensorIds = new();

    // Base values for sine wave simulation
    private static readonly Dictionary<string, (double Base, double Amplitude, double Period, double Noise)> ParamConfigs = new()
    {
        ["pH"] = (7.0, 1.5, 30, 0.2),
        ["Turbidity"] = (5.0, 8.0, 45, 3.0),
        ["DissolvedOxygen"] = (8.0, 2.0, 60, 0.3),
        ["Temperature"] = (22.0, 5.0, 20, 0.5),
        ["Conductivity"] = (450, 100, 50, 20)
    };

    public SensorSimulatorHostedService(IServiceScopeFactory scopeFactory, IHubContext<WaterDataHub> hubContext, ILogger<SensorSimulatorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sensor Simulator starting...");

        // Wait for startup
        await Task.Delay(5000, stoppingToken);

        // Ensure at least 2 demo sensors exist
        await EnsureDemoSensorsExist(stoppingToken);

        var tick = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                tick++;
                await SimulateReadings(tick, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sensor simulation");
            }

            await Task.Delay(3000, stoppingToken); // Every 3 seconds
        }
    }

    private async Task EnsureDemoSensorsExist(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
        var sensors = (await sensorRepo.GetAllAsync(isActive: true)).ToList();

        if (!sensors.Any())
        {
            var demoSensors = new[]
            {
                new Sensor { Id = Guid.NewGuid(), Name = "Sensor Río Medellín", Location = "Río Medellín - Estación Sur", Latitude = 6.2308m, Longitude = -75.5906m, Description = "Estación de monitoreo en la cuenca sur del Río Medellín" },
                new Sensor { Id = Guid.NewGuid(), Name = "Sensor Embalse Peñol", Location = "Embalse Peñol-Guatapé - Estación Norte", Latitude = 6.2597m, Longitude = -75.1473m, Description = "Estación en el embalse principal de generación hidroeléctrica" },
                new Sensor { Id = Guid.NewGuid(), Name = "Sensor Río Cauca", Location = "Río Cauca - Puerto Valdivia", Latitude = 7.1167m, Longitude = -75.4167m, Description = "Estación de monitoreo en la cuenca baja del Río Cauca" }
            };

            foreach (var sensor in demoSensors)
            {
                await sensorRepo.CreateAsync(sensor);
                _logger.LogInformation("Created demo sensor: {Name}", sensor.Name);
            }
        }

        // Refresh sensor IDs
        using var scope2 = _scopeFactory.CreateScope();
        var repo2 = scope2.ServiceProvider.GetRequiredService<ISensorRepository>();
        var activeSensors = (await repo2.GetAllAsync(isActive: true)).ToList();
        _sensorIds.Clear();
        _sensorIds.AddRange(activeSensors.Select(s => s.Id));
    }

    private async Task SimulateReadings(int tick, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var readingRepo = scope.ServiceProvider.GetRequiredService<IReadingRepository>();

        // Refresh sensor list periodically
        if (tick % 20 == 0)
        {
            var sensorRepo = scope.ServiceProvider.GetRequiredService<ISensorRepository>();
            var activeSensors = (await sensorRepo.GetAllAsync(isActive: true)).ToList();
            _sensorIds.Clear();
            _sensorIds.AddRange(activeSensors.Select(s => s.Id));
        }

        if (!_sensorIds.Any()) return;

        foreach (var sensorId in _sensorIds)
        {
            var reading = GenerateReading(sensorId, tick);

            // Random spike every ~15 seconds (every 5 ticks per sensor) to trigger alerts
            if (tick % 5 == _random.Next(_sensorIds.Count))
            {
                var spikeParam = _random.Next(5) switch
                {
                    0 => "pH",
                    1 => "Turbidity",
                    2 => "DissolvedOxygen",
                    3 => "Temperature",
                    _ => "Conductivity"
                };

                ApplySpikeInPlace(reading, spikeParam);
            }

            var created = await readingRepo.CreateAsync(reading);

            // Broadcast via SignalR
            var dto = new SensorReadingDto(
                created.Id, created.SensorId, "Simulated", "Simulated",
                created.Ph, created.Turbidity, created.DissolvedOxygen,
                created.Temperature, created.Conductivity, created.Timestamp
            );

            await _hubContext.Clients.All.SendAsync("ReceiveReading", dto, ct);
        }
    }

    private SensorReading GenerateReading(Guid sensorId, int tick)
    {
        double Sine(string param)
        {
            var cfg = ParamConfigs[param];
            return cfg.Base + Math.Sin(tick / cfg.Period) * cfg.Amplitude + (_random.NextDouble() - 0.5) * cfg.Noise;
        }

        return new SensorReading
        {
            Id = Guid.NewGuid(),
            SensorId = sensorId,
            Ph = Math.Round((decimal)Sine("pH"), 2),
            Turbidity = Math.Round((decimal)Math.Max(0, Sine("Turbidity")), 2),
            DissolvedOxygen = Math.Round((decimal)Math.Max(0, Sine("DissolvedOxygen")), 2),
            Temperature = Math.Round((decimal)Sine("Temperature"), 2),
            Conductivity = Math.Round((decimal)Math.Max(0, Sine("Conductivity")), 2),
            Timestamp = DateTime.UtcNow
        };
    }

    private static void ApplySpikeInPlace(SensorReading reading, string parameter)
    {
        switch (parameter)
        {
            case "pH": reading.Ph = 11.5m; break;
            case "Turbidity": reading.Turbidity = 180.0m; break;
            case "DissolvedOxygen": reading.DissolvedOxygen = 3.0m; break;
            case "Temperature": reading.Temperature = 35.0m; break;
            case "Conductivity": reading.Conductivity = 1200m; break;
        }
    }
}
