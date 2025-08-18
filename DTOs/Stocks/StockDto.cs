using server.DTOs.Comment;
using server.DTOs.Transaction;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.DTOs.Stocks
{
    public class StockDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal Purchase { get; set; }
        public decimal? CurrentPrice { get; set; }
        public long MarketCap { get; set; }
        public decimal LastDiv { get; set; }
        public string Industry { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal? PERatio { get; set; }
        public decimal? DividendYield { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }

    public class CreateStockRequestDto
    {
        [Required]
        [StringLength(100)]
        [MaxLength(10, ErrorMessage = "CompanyName cannot be over 10 over characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [MaxLength(10, ErrorMessage = "Symbol cannot be over 10 over characters")]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Purchase { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long MarketCap { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LastDiv { get; set; }

        [Required]
        [StringLength(50)]
        public string Industry { get; set; } = string.Empty;

        [StringLength(20)]
        public string Country { get; set; } = "US";
    }

    public class UpdateStockRequestDto
    {
        [Required]
        [StringLength(100)]
        [MaxLength(10, ErrorMessage = "CompanyName cannot be over 10 over characters")]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [MaxLength(10, ErrorMessage = "Symbol cannot be over 10 over characters")]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Purchase { get; set; }

        [Required]
        [Range(1, long.MaxValue)]
        public long MarketCap { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LastDiv { get; set; }

        [Required]
        [StringLength(50)]
        public string Industry { get; set; } = string.Empty;

        [StringLength(20)]
        public string Country { get; set; } = "US";
    }
}
