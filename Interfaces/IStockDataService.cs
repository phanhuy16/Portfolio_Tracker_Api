using server.Models;

namespace server.Interfaces
{
    public interface IStockDataService
    {
        Task<Stock?> GetStockDataFromFmpAsync(string symbol);
        Task<bool> PopulateStocksFromSymbolListAsync(List<string> symbols);
        Task<bool> UpdateStockFundamentalsAsync(string symbol);
        Task<List<string>> GetPopularStockSymbolsAsync();
        Task SeedPopularStocksAsync();
    }
}
