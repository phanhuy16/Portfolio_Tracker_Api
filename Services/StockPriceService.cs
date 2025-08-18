using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Models;

namespace server.Services
{
    public class StockPriceService : IStockPriceService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;


        public StockPriceService(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetCurrentPriceAsync(string symbol)
        {
            // Mock implementation - replace with real API call
            var random = new Random();
            var basePrice = await _context.Stocks
                .Where(s => s.Symbol == symbol)
                .Select(s => s.Purchase)
                .FirstOrDefaultAsync();

            if (basePrice == 0) return null;

            // Simulate price movement (+/- 10%)
            var variation = (decimal)(random.NextDouble() * 0.2 - 0.1);
            return basePrice * (1 + variation);
        }

        public async Task UpdateAllStockPricesAsync()
        {
            var stocks = await _context.Stocks.Where(s => s.IsActive).ToListAsync();

            foreach (var stock in stocks)
            {
                var currentPrice = await GetCurrentPriceAsync(stock.Symbol);
                if (currentPrice.HasValue)
                {
                    stock.CurrentPrice = currentPrice.Value;
                    stock.LastUpdated = DateTime.Now;

                    // Save historical price
                    var stockPrice = new StockPrice
                    {
                        StockId = stock.Id,
                        Open = currentPrice.Value,
                        High = currentPrice.Value * 1.02m,
                        Low = currentPrice.Value * 0.98m,
                        Close = currentPrice.Value,
                        Volume = new Random().Next(1000000, 10000000),
                        Date = DateTime.Today
                    };

                    await SaveStockPriceAsync(stockPrice);

                    // Update portfolio current prices
                    var portfolios = await _context.Portfolios
                        .Where(p => p.StockId == stock.Id)
                        .ToListAsync();

                    foreach (var portfolio in portfolios)
                    {
                        portfolio.CurrentPrice = currentPrice.Value;
                        portfolio.LastUpdated = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<StockPrice>> GetHistoricalPricesAsync(int stockId, DateTime startDate, DateTime endDate)
        {
            return await _context.StockPrices
                .Where(sp => sp.StockId == stockId && sp.Date >= startDate && sp.Date <= endDate)
                .OrderBy(sp => sp.Date)
                .ToListAsync();
        }

        public async Task SaveStockPriceAsync(StockPrice stockPrice)
        {
            var existingPrice = await _context.StockPrices
                .FirstOrDefaultAsync(sp => sp.StockId == stockPrice.StockId && sp.Date.Date == stockPrice.Date.Date);

            if (existingPrice != null)
            {
                existingPrice.Open = stockPrice.Open;
                existingPrice.High = stockPrice.High;
                existingPrice.Low = stockPrice.Low;
                existingPrice.Close = stockPrice.Close;
                existingPrice.Volume = stockPrice.Volume;
            }
            else
            {
                await _context.StockPrices.AddAsync(stockPrice);
            }
        }
    }
}
