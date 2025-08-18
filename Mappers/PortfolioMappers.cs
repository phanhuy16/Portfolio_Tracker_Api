using server.DTOs.Portfolio;
using server.Models;

namespace server.Mappers
{
    public static class PortfolioMappers
    {
        public static PortfolioDto ToPortfolioDto(this Portfolio portfolio)
        {
            return new PortfolioDto
            {
                Id = portfolio.Id,
                Symbol = portfolio.Stock.Symbol ?? "",
                CompanyName = portfolio.Stock.CompanyName ?? "",
                Quantity = portfolio.Quantity,
                PurchasePrice = portfolio.PurchasePrice,
                PurchaseDate = portfolio.PurchaseDate,
                CurrentPrice = portfolio.CurrentPrice,
                TotalCost = portfolio.TotalCost,
                CurrentValue = portfolio.CurrentValue,
                ProfitLoss = portfolio.ProfitLoss,
                ProfitLossPercentage = portfolio.ProfitLossPercentage,
                AppUser = portfolio.AppUser.MapToAppUser(),
                Stock = portfolio.Stock.ToStockDto()
            };
        }

        public static Portfolio ToPortfolioFromCreate(this CreatePortfolioDto createPortfolioDto, string userId)
        {
            return new Portfolio
            {
                UserId = userId,
                StockId = createPortfolioDto.StockId,
                Quantity = createPortfolioDto.Quantity,
                PurchasePrice = createPortfolioDto.PurchasePrice,
                PurchaseDate = createPortfolioDto.PurchaseDate
            };
        }
    }
}
