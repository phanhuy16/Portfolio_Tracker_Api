using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Portfolios")]
    public class Portfolio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int StockId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PurchasePrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CurrentPrice { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
        public Stock Stock { get; set; } = null!;

        // Calculated properties
        [NotMapped]
        public decimal TotalCost => Quantity * PurchasePrice;

        [NotMapped]
        public decimal? CurrentValue => CurrentPrice.HasValue ? Quantity * CurrentPrice.Value : null;

        [NotMapped]
        public decimal? ProfitLoss => CurrentValue.HasValue ? CurrentValue.Value - TotalCost : null;

        [NotMapped]
        public decimal? ProfitLossPercentage => CurrentValue.HasValue && TotalCost > 0
            ? ((CurrentValue.Value - TotalCost) / TotalCost) * 100 : null;
    }
}
