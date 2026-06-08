using Microsoft.EntityFrameworkCore;
using AlertService.Models;

namespace AlertService.Data;

public class AlertDbContext : DbContext
{
    public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options) { }

    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.HasIndex(r => r.ParameterName);
            entity.HasIndex(r => r.IsActive);
            entity.HasIndex(r => r.Severity);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasIndex(a => a.CreatedAt).IsDescending();
            entity.HasIndex(a => a.IsAcknowledged).HasFilter("[IsAcknowledged] = 0");
            entity.HasIndex(a => a.ParameterName);
            entity.HasIndex(a => a.Severity);
            entity.HasIndex(a => a.SensorId);

            entity.HasOne(a => a.AlertRule)
                  .WithMany(r => r.Alerts)
                  .HasForeignKey(a => a.AlertRuleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
