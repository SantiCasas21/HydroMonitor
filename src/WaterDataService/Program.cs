using Microsoft.EntityFrameworkCore;
using Serilog;
using WaterDataService.Data;
using WaterDataService.Hubs;
using WaterDataService.Repositories;
using WaterDataService.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// PostgreSQL
builder.Services.AddDbContext<WaterDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();

// Services
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IReadingService, ReadingService>();

// Hosted Services
builder.Services.AddHostedService<SensorSimulatorHostedService>();

// SignalR
builder.Services.AddSignalR();

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WaterDataService API", Version = "v1", Description = "Water Quality Data Microservice - Sensor readings & real-time streaming" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WaterDataDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Log.Information("Database migrated successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not run migrations. If database is unavailable, the service will retry.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WaterDataService v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseSerilogRequestLogging();
app.MapControllers();
app.MapHub<WaterDataHub>("/hubs/waterdata");

Log.Information("WaterDataService starting on port 5001...");
app.Run();
