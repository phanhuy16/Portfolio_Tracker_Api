using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Stocks")]
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Purchase { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CurrentPrice { get; set; }

        public long MarketCap { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LastDiv { get; set; }

        [StringLength(50)]
        public string Industry { get; set; } = string.Empty;

        [StringLength(20)]
        public string Country { get; set; } = "US";

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? PERatio { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? DividendYield { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
        public List<StockPrice> StockPrices { get; set; } = new List<StockPrice>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
