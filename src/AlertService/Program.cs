using Microsoft.EntityFrameworkCore;
using Serilog;
using AlertService.Data;
using AlertService.Repositories;
using AlertService.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// MSSQL
builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client for polling WaterDataService
builder.Services.AddHttpClient("WaterDataService", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Repositories
builder.Services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();

// Services
builder.Services.AddScoped<IAlertRuleService, AlertRuleService>();
builder.Services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();

// Hosted Services
builder.Services.AddHostedService<WaterDataPollingService>();

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AlertService API", Version = "v1", Description = "Water Quality Alert Microservice - Threshold monitoring & alert management" });
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
    var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        Log.Information("AlertService database migrated successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Could not run migrations. If database is unavailable, the service will retry.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AlertService v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseSerilogRequestLogging();
app.MapControllers();

Log.Information("AlertService starting on port 5002...");
app.Run();
