using System.Net.Http.Json;
using System.Text.Json;
using AlertService.Repositories;

namespace AlertService.Services;

public class WaterDataPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WaterDataPollingService> _logger;

    public WaterDataPollingService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WaterDataPollingService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WaterData Polling Service starting...");
        await Task.Delay(10000, stoppingToken); // Initial delay for services to start

        // Ensure demo alert rules exist
        await EnsureDemoRulesExist(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndEvaluate(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in polling service");
            }

            await Task.Delay(5000, stoppingToken); // Poll every 5 seconds
        }
    }

    private async Task EnsureDemoRulesExist(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var ruleRepo = scope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();
        var rules = (await ruleRepo.GetAllAsync()).ToList();

        if (!rules.Any())
        {
            var demoRules = new[]
            {
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "pH", MaxThreshold = 8.5m, Severity = "Warning", Description = "pH above safe drinking water range", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "pH", MaxThreshold = 10.0m, Severity = "Critical", Description = "pH dangerously high - corrosive water", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "Turbidity", MaxThreshold = 5.0m, Severity = "Warning", Description = "Turbidity above safe level for drinking water", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "Turbidity", MaxThreshold = 50.0m, Severity = "Critical", Description = "Extremely high turbidity - possible contamination", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "DissolvedOxygen", MaxThreshold = 12.0m, Severity = "Warning", Description = "DO above normal range - possible algal bloom", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "DissolvedOxygen", MinThreshold = 4.0m, Severity = "Critical", Description = "Dissolved oxygen critically low - aquatic life at risk", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "Temperature", MaxThreshold = 28.0m, Severity = "Warning", Description = "Water temperature elevated - thermal pollution", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "Temperature", MaxThreshold = 33.0m, Severity = "Critical", Description = "Water temperature critical - ecosystem stress", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Models.AlertRule { Id = Guid.NewGuid(), ParameterName = "Conductivity", MaxThreshold = 800m, Severity = "Warning", Description = "Conductivity elevated - possible dissolved solids pollution", IsActive = true, CreatedAt = DateTime.UtcNow },
            };

            foreach (var rule in demoRules)
            {
                await ruleRepo.CreateAsync(rule);
                _logger.LogInformation("Created demo rule: {Desc}", rule.Description);
            }
        }
    }

    private async Task PollAndEvaluate(CancellationToken ct)
    {
        var baseUrl = _configuration["WaterDataService:BaseUrl"] ?? "http://localhost:5001";
        var client = _httpClientFactory.CreateClient("WaterDataService");
        client.BaseAddress = new Uri(baseUrl);

        var response = await client.GetAsync("/api/readings/latest", ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to poll WaterDataService: {StatusCode}", response.StatusCode);
            return;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var readings = await response.Content.ReadFromJsonAsync<List<SensorReadingData>>(jsonOptions, ct);

        if (readings == null || readings.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var evaluationService = scope.ServiceProvider.GetRequiredService<IAlertEvaluationService>();

        foreach (var reading in readings)
        {
            var alerts = await evaluationService.EvaluateReadingAsync(reading);
            foreach (var alert in alerts)
            {
                _logger.LogInformation("Alert generated: {Message}", alert.Message);
            }
        }
    }
}
