using server.DTOs.Auth;
using server.DTOs.Stocks;
using server.Models;
using System.ComponentModel.DataAnnotations;

namespace server.DTOs.Portfolio
{
    public class PortfolioDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal? CurrentPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal? CurrentValue { get; set; }
        public decimal? ProfitLoss { get; set; }
        public decimal? ProfitLossPercentage { get; set; }
        public AppUserDto AppUser { get; set; } = default!;
        public StockDto Stock { get; set; } = default!;
    }

    public class CreatePortfolioDto
    {
        [Required]
        public int StockId { get; set; }

        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;
    }

    public class UpdatePortfolioDto
    {
        [Required]
        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal PurchasePrice { get; set; }
    }

    public class PortfolioSummaryDto
    {
        public decimal TotalInvestment { get; set; }
        public decimal? TotalCurrentValue { get; set; }
        public decimal? TotalProfitLoss { get; set; }
        public decimal? TotalProfitLossPercentage { get; set; }
        public int TotalStocks { get; set; }
        public List<PortfolioDto> Holdings { get; set; } = new List<PortfolioDto>();
        public List<TopPerformerDto> TopPerformers { get; set; } = new List<TopPerformerDto>();
        public List<TopPerformerDto> WorstPerformers { get; set; } = new List<TopPerformerDto>();
    }

    public class TopPerformerDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal? ProfitLoss { get; set; }
        public decimal? ProfitLossPercentage { get; set; }
    }
}
