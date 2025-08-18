using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("StockPrices")]
    public class StockPrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StockId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Open { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal High { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Low { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Close { get; set; }

        public long Volume { get; set; }

        public DateTime Date { get; set; }

        // Navigation property
        public Stock Stock { get; set; } = null!;
    }
}
