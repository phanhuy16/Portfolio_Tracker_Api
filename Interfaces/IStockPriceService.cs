using server.Models;

namespace server.Interfaces
{
    public interface IStockPriceService
    {
        Task<decimal?> GetCurrentPriceAsync(string symbol);
        Task UpdateAllStockPricesAsync();
        Task<List<StockPrice>> GetHistoricalPricesAsync(int stockId, DateTime startDate, DateTime endDate);
        Task SaveStockPriceAsync(StockPrice stockPrice);
    }
}
