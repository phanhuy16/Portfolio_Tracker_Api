using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    public enum TransactionType
    {
        Buy = 1,
        Sell = 2,
        Dividend = 3
    }

    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int StockId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Commission { get; set; } = 0;

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public AppUser AppUser { get; set; } = null!;
        public Stock Stock { get; set; } = null!;

        // Calculated properties
        [NotMapped]
        public decimal TotalAmount => (Quantity * Price) + Commission;
    }
}
