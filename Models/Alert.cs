using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    public enum AlertType
    {
        PriceAbove = 1,
        PriceBelow = 2,
        VolumeSpike = 3,
        PercentageChange = 4
    }

    [Table("Alerts")]
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int StockId { get; set; }

        [Required]
        public AlertType AlertType { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TriggerValue { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsTriggered { get; set; } = false;

        public DateTime? TriggeredDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
        public Stock Stock { get; set; } = null!;
    }
}
