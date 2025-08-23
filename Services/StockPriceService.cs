using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Fmp;
using server.DTOs.YahooFinance;
using server.Interfaces;
using server.Models;
using System.Text.Json;

namespace server.Services
{
    public class StockPriceService : IStockPriceService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockPriceService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _fmpApiKey;
        private readonly string _fmpBaseUrl = "https://financialmodelingprep.com/api/v3/";

        public StockPriceService(
            ApplicationDbContext context,
            HttpClient httpClient,
            ILogger<StockPriceService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            _fmpApiKey = _configuration["FMP:ApiKey"] ?? throw new InvalidOperationException("FMP API Key not found in configuration");

            // Configure HttpClient
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "StockApp/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<decimal?> GetCurrentPriceAsync(string symbol)
        {
            try
            {
                var url = $"{_fmpBaseUrl}quote-short/{symbol}?apikey={_fmpApiKey}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get current price for {symbol}: {response.StatusCode}");
                    return await GetMockPriceAsync(symbol);
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var priceData = JsonSerializer.Deserialize<FmpRealtimePrice[]>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (priceData != null && priceData.Length > 0 && priceData[0].Price > 0)
                {
                    _logger.LogDebug($"Got REAL FMP price for {symbol}: ${priceData[0].Price}");
                    return priceData[0].Price;
                }

                _logger.LogWarning($"No valid FMP price data found for {symbol}, using mock");
                return await GetMockPriceAsync(symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting FMP price for {symbol}, falling back to mock");
                return await GetMockPriceAsync(symbol);
            }
        }

        public async Task<StockPrice?> GetRealOHLCVAsync(string symbol, int stockId, DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.Today;

                // FMP historical data endpoint - lấy 5 ngày gần nhất
                var fromDate = targetDate.AddDays(-5).ToString("yyyy-MM-dd");
                var toDate = targetDate.ToString("yyyy-MM-dd");

                var url = $"{_fmpBaseUrl}historical-price-full/{symbol}?from={fromDate}&to={toDate}&apikey={_fmpApiKey}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get OHLCV for {symbol} from FMP");
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var historicalData = JsonSerializer.Deserialize<FmpHistoricalPriceResponse>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (historicalData?.Historical != null && historicalData.Historical.Count > 0)
                {
                    // Lấy ngày gần nhất có data
                    var latestData = historicalData.Historical
                        .Where(h => h.Volume > 0) // Ensure we have volume data
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();

                    if (latestData != null)
                    {
                        var stockPrice = new StockPrice
                        {
                            StockId = stockId,
                            Open = latestData.Open,
                            High = latestData.High,
                            Low = latestData.Low,
                            Close = latestData.Close,
                            Volume = latestData.Volume,
                            Date = latestData.Date.Date
                        };

                        _logger.LogDebug($"Got REAL FMP OHLCV for {symbol} on {stockPrice.Date}: O:{stockPrice.Open} H:{stockPrice.High} L:{stockPrice.Low} C:{stockPrice.Close} V:{stockPrice.Volume}");
                        return stockPrice;
                    }
                }

                _logger.LogWarning($"No valid FMP OHLCV data found for {symbol}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting FMP OHLCV for {symbol}");
                return null;
            }
        }

        public async Task UpdateAllStockPricesAsync()
        {
            try
            {
                var stocks = await _context.Stocks.Where(s => s.IsActive).ToListAsync();
                var updateCount = 0;
                var realDataCount = 0;

                _logger.LogInformation($"Starting to update prices for {stocks.Count} stocks using FMP API");

                // FMP supports bulk requests, but let's do individual for better error handling
                foreach (var stock in stocks)
                {
                    try
                    {
                        var currentPrice = await GetCurrentPriceAsync(stock.Symbol);

                        if (currentPrice.HasValue)
                        {
                            // Update stock current price
                            stock.CurrentPrice = currentPrice.Value;
                            stock.LastUpdated = DateTime.UtcNow;

                            // Try to get REAL OHLCV data
                            var realOHLCV = await GetRealOHLCVAsync(stock.Symbol, stock.Id);

                            if (realOHLCV != null)
                            {
                                await SaveStockPriceAsync(realOHLCV);
                                realDataCount++;
                                _logger.LogDebug($"Saved REAL FMP OHLCV for {stock.Symbol}");
                            }
                            else
                            {
                                // Fallback to estimated data
                                var estimatedPrice = new StockPrice
                                {
                                    StockId = stock.Id,
                                    Open = currentPrice.Value * 0.995m,
                                    High = currentPrice.Value * 1.02m,
                                    Low = currentPrice.Value * 0.98m,
                                    Close = currentPrice.Value,
                                    Volume = 1000000,
                                    Date = DateTime.Today
                                };
                                await SaveStockPriceAsync(estimatedPrice);
                                _logger.LogDebug($"Saved ESTIMATED data for {stock.Symbol}");
                            }

                            // Update portfolio current prices
                            var portfolios = await _context.Portfolios
                                .Where(p => p.StockId == stock.Id)
                                .ToListAsync();

                            foreach (var portfolio in portfolios)
                            {
                                portfolio.CurrentPrice = currentPrice.Value;
                                portfolio.LastUpdated = DateTime.UtcNow;
                            }

                            updateCount++;
                        }
                        else
                        {
                            _logger.LogWarning($"Could not get FMP price for {stock.Symbol}");
                        }

                        // Rate limiting - FMP free plan has limits
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating FMP price for {stock.Symbol}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated {updateCount} stocks using FMP. Real OHLCV data: {realDataCount}, Estimated: {updateCount - realDataCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FMP UpdateAllStockPricesAsync");
                throw;
            }
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
            try
            {
                var existingPrice = await _context.StockPrices
                    .FirstOrDefaultAsync(sp => sp.StockId == stockPrice.StockId && sp.Date.Date == stockPrice.Date.Date);

                if (existingPrice != null)
                {
                    // Update existing price
                    existingPrice.Open = stockPrice.Open;
                    existingPrice.High = stockPrice.High;
                    existingPrice.Low = stockPrice.Low;
                    existingPrice.Close = stockPrice.Close;
                    existingPrice.Volume = stockPrice.Volume;
                    _logger.LogDebug($"Updated existing price record for StockId: {stockPrice.StockId}");
                }
                else
                {
                    await _context.StockPrices.AddAsync(stockPrice);
                    _logger.LogDebug($"Added new price record for StockId: {stockPrice.StockId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving stock price for StockId: {stockPrice.StockId}");
                throw;
            }
        }

        public async Task<object?> GetDetailedStockInfoAsync(string symbol)
        {
            try
            {
                var url = $"{_fmpBaseUrl}quote/{symbol}?apikey={_fmpApiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var stockData = JsonSerializer.Deserialize<object[]>(jsonString);

                return stockData?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting detailed FMP stock info for {symbol}");
                return null;
            }
        }

        public async Task<bool> ForceUpdateWithRealDataAsync(string symbol)
        {
            try
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpper());
                if (stock == null)
                {
                    _logger.LogWarning($"Stock {symbol} not found");
                    return false;
                }

                var currentPrice = await GetCurrentPriceAsync(symbol);
                var realOHLCV = await GetRealOHLCVAsync(symbol, stock.Id);

                if (currentPrice.HasValue)
                {
                    stock.CurrentPrice = currentPrice.Value;
                    stock.LastUpdated = DateTime.UtcNow;
                }

                if (realOHLCV != null)
                {
                    await SaveStockPriceAsync(realOHLCV);
                    _logger.LogInformation($"Successfully updated {symbol} with REAL FMP data");
                    await _context.SaveChangesAsync();
                    return true;
                }

                _logger.LogWarning($"Could not get real FMP OHLCV data for {symbol}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error force updating {symbol} with FMP");
                return false;
            }
        }

        // Batch update method - FMP supports multiple symbols in one request
        public async Task<bool> BatchUpdatePricesAsync(List<string> symbols)
        {
            try
            {
                if (symbols == null || symbols.Count == 0)
                    return false;

                // FMP allows multiple symbols separated by comma (up to 100 symbols per request)
                var batches = symbols.Chunk(50).ToList(); // Process in batches of 50 to be safe
                var totalUpdated = 0;

                foreach (var batch in batches)
                {
                    var symbolsString = string.Join(",", batch);
                    var url = $"{_fmpBaseUrl}quote/{symbolsString}?apikey={_fmpApiKey}";

                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var priceData = JsonSerializer.Deserialize<FmpRealtimePrice[]>(jsonString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (priceData != null)
                        {
                            foreach (var price in priceData)
                            {
                                var stock = await _context.Stocks
                                    .FirstOrDefaultAsync(s => s.Symbol == price.Symbol.ToUpper());

                                if (stock != null && price.Price > 0)
                                {
                                    stock.CurrentPrice = price.Price;
                                    stock.LastUpdated = DateTime.UtcNow;
                                    totalUpdated++;
                                }
                            }
                        }
                    }

                    // Rate limiting between batches
                    await Task.Delay(500);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Batch updated {totalUpdated} stocks using FMP API");
                return totalUpdated > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FMP batch update");
                return false;
            }
        }

        // Method để lấy bulk historical data
        public async Task<bool> BulkUpdateHistoricalDataAsync(List<string> symbols, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var successCount = 0;

                foreach (var symbol in symbols)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpper());
                    if (stock == null) continue;

                    var from = fromDate.ToString("yyyy-MM-dd");
                    var to = toDate.ToString("yyyy-MM-dd");
                    var url = $"{_fmpBaseUrl}historical-price-full/{symbol}?from={from}&to={to}&apikey={_fmpApiKey}";

                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var historicalData = JsonSerializer.Deserialize<FmpHistoricalPriceResponse>(jsonString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (historicalData?.Historical != null)
                        {
                            foreach (var dailyPrice in historicalData.Historical)
                            {
                                var stockPrice = new StockPrice
                                {
                                    StockId = stock.Id,
                                    Open = dailyPrice.Open,
                                    High = dailyPrice.High,
                                    Low = dailyPrice.Low,
                                    Close = dailyPrice.Close,
                                    Volume = dailyPrice.Volume,
                                    Date = dailyPrice.Date.Date
                                };

                                await SaveStockPriceAsync(stockPrice);
                            }
                            successCount++;
                        }
                    }

                    // Rate limiting
                    await Task.Delay(300);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Bulk updated historical data for {successCount} stocks");
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk historical data update");
                return false;
            }
        }

        private async Task<decimal?> GetMockPriceAsync(string symbol)
        {
            _logger.LogInformation($"Using mock price for {symbol}");

            var basePrice = await _context.Stocks
                .Where(s => s.Symbol == symbol)
                .Select(s => s.Purchase)
                .FirstOrDefaultAsync();

            if (basePrice == 0) return null;

            var random = new Random();
            var variation = (decimal)(random.NextDouble() * 0.04 - 0.02); // +/- 2%
            return basePrice * (1 + variation);
        }
    }
}
