using server.DTOs.Stocks;
using server.Models;

namespace server.Mappers
{
    public static class StockMapper
    {
        public static StockDto ToStockDto(this Stock stockModel)
        {
            return new StockDto
            {
                Id = stockModel.Id,
                Symbol = stockModel.Symbol,
                CompanyName = stockModel.CompanyName,
                Purchase = stockModel.Purchase,
                CurrentPrice = stockModel.CurrentPrice,
                LastDiv = stockModel.LastDiv,
                Industry = stockModel.Industry,
                MarketCap = stockModel.MarketCap,
                Country = stockModel.Country,
                PERatio = stockModel.PERatio,
                DividendYield = stockModel.DividendYield,
                LastUpdated = stockModel.LastUpdated,
                Comments = stockModel.Comments.Select(c => c.ToCommentDto()).ToList(),
                Transactions = stockModel.Transactions.Select(t => t.ToTransactionDto()).ToList()
            };
        }

        public static Stock ToStockFromCreateDTO(this CreateStockRequestDto stockDto)
        {
            return new Stock
            {
                Symbol = stockDto.Symbol,
                CompanyName = stockDto.CompanyName,
                Purchase = stockDto.Purchase,
                LastDiv = stockDto.LastDiv,
                Industry = stockDto.Industry,
                MarketCap = stockDto.MarketCap,
                Country = stockDto.Country
            };
        }
    }
}
