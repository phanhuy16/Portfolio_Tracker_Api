using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Transaction
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Commission { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateTransactionDto
    {
        [Required]
        public int StockId { get; set; }

        [Required]
        public int TransactionType { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Commission { get; set; } = 0;

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
