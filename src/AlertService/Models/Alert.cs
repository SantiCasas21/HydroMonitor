using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlertService.Models;

public class Alert
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? AlertRuleId { get; set; }

    [ForeignKey(nameof(AlertRuleId))]
    public AlertRule? AlertRule { get; set; }

    [Required]
    public Guid SensorId { get; set; }

    [Required]
    public Guid ReadingId { get; set; }

    [Required, MaxLength(50)]
    public string ParameterName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,3)")]
    public decimal ActualValue { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? MinThreshold { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? MaxThreshold { get; set; }

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Warning";

    public bool IsAcknowledged { get; set; } = false;

    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(100)]
    public string? AcknowledgedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
