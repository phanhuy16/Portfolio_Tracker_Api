using server.DTOs.YahooFinance;
using server.Models;

namespace server.Interfaces
{
    public interface IStockPriceService
    {
        Task<decimal?> GetCurrentPriceAsync(string symbol);
        Task UpdateAllStockPricesAsync();
        Task<List<StockPrice>> GetHistoricalPricesAsync(int stockId, DateTime startDate, DateTime endDate);
        Task SaveStockPriceAsync(StockPrice stockPrice);
        Task<object?> GetDetailedStockInfoAsync(string symbol);
        Task<StockPrice?> GetRealOHLCVAsync(string symbol, int stockId, DateTime? date = null);
        Task<bool> ForceUpdateWithRealDataAsync(string symbol);
        Task<bool> BatchUpdatePricesAsync(List<string> symbols);
        Task<bool> BulkUpdateHistoricalDataAsync(List<string> symbols, DateTime fromDate, DateTime toDate);
    }
}
