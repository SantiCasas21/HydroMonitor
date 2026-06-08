using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaterDataService.Models;

public class SensorReading
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SensorId { get; set; }

    [ForeignKey(nameof(SensorId))]
    public Sensor? Sensor { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    public decimal Ph { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal Turbidity { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal DissolvedOxygen { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Temperature { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal Conductivity { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
