using Microsoft.EntityFrameworkCore;
using WaterDataService.Models;

namespace WaterDataService.Data;

public class WaterDataDbContext : DbContext
{
    public WaterDataDbContext(DbContextOptions<WaterDataDbContext> options) : base(options) { }

    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasIndex(s => s.Name);
            entity.HasIndex(s => s.IsActive);
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasIndex(r => r.SensorId);
            entity.HasIndex(r => r.Timestamp).IsDescending();
            entity.HasIndex(r => new { r.SensorId, r.Timestamp }).IsDescending();

            entity.HasOne(r => r.Sensor)
                  .WithMany(s => s.Readings)
                  .HasForeignKey(r => r.SensorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
