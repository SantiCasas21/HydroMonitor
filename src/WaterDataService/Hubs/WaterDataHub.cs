using Microsoft.AspNetCore.SignalR;

namespace WaterDataService.Hubs;

public class WaterDataHub : Hub
{
    private readonly ILogger<WaterDataHub> _logger;

    public WaterDataHub(ILogger<WaterDataHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to WaterDataHub: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from WaterDataHub: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToSensor(string sensorId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"sensor-{sensorId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to sensor {SensorId}", Context.ConnectionId, sensorId);
    }

    public async Task UnsubscribeFromSensor(string sensorId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sensor-{sensorId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from sensor {SensorId}", Context.ConnectionId, sensorId);
    }
}
