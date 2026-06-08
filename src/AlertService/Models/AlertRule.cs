using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlertService.Models;

public class AlertRule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string ParameterName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,3)")]
    public decimal? MinThreshold { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? MaxThreshold { get; set; }

    [Required, MaxLength(20)]
    public string Severity { get; set; } = "Warning";

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
