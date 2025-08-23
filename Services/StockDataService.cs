using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Fmp;
using server.DTOs.YahooFinance;
using server.Interfaces;
using server.Models;
using System.Text.Json;

namespace server.Services
{
    public class StockDataService : IStockDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockDataService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _fmpApiKey;
        private readonly string _fmpBaseUrl = "https://financialmodelingprep.com/api/v3/";

        public StockDataService(
            ApplicationDbContext context,
            HttpClient httpClient,
            ILogger<StockDataService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // Lấy API key từ configuration
            _fmpApiKey = _configuration["FMP:ApiKey"] ?? throw new InvalidOperationException("FMP API Key not found in configuration");

            // Configure HttpClient
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "StockApp/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<string>> GetPopularStockSymbolsAsync()
        {
            // List of popular stocks to populate your database
            return await Task.FromResult(new List<string>
            {
                // Tech Giants
                "AAPL", "GOOGL", "GOOG", "MSFT", "AMZN", "META", "NFLX", "TSLA",
                "NVDA", "AMD", "INTC", "ORCL", "CRM", "ADBE", "PYPL",

                // Finance
                "JPM", "BAC", "WFC", "GS", "MS", "C", "BRK-B", "V", "MA",

                // Healthcare & Pharma
                "JNJ", "PFE", "UNH", "ABBV", "LLY", "BMY", "MRK", "TMO",

                // Consumer
                "KO", "PEP", "WMT", "TGT", "HD", "LOW", "MCD", "SBUX", "NKE",

                // Industrial & Energy
                "BA", "CAT", "GE", "XOM", "CVX", "COP", "SLB",

                // Telecom & Utilities
                "VZ", "T", "TMUS", "NEE", "DUK",

                // REITs & Others
                "SPY", "QQQ", "IWM", "DIA", "VTI"
            });
        }

        public async Task<Stock?> GetStockDataFromFmpAsync(string symbol)
        {
            try
            {
                // Lấy quote data (giá hiện tại, P/E, etc.)
                var quoteUrl = $"{_fmpBaseUrl}quote/{symbol}?apikey={_fmpApiKey}";
                var profileUrl = $"{_fmpBaseUrl}profile/{symbol}?apikey={_fmpApiKey}";

                _logger.LogInformation($"Fetching FMP data for {symbol}");

                // Parallel requests để tối ưu performance
                var quoteTask = _httpClient.GetAsync(quoteUrl);
                var profileTask = _httpClient.GetAsync(profileUrl);

                await Task.WhenAll(quoteTask, profileTask);

                var quoteResponse = await quoteTask;
                var profileResponse = await profileTask;

                FmpQuoteResponse[]? quoteData = null;
                FmpProfileResponse[]? profileData = null;

                // Parse quote data
                if (quoteResponse.IsSuccessStatusCode)
                {
                    var quoteJson = await quoteResponse.Content.ReadAsStringAsync();
                    quoteData = JsonSerializer.Deserialize<FmpQuoteResponse[]>(quoteJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed to get quote data for {symbol}: {quoteResponse.StatusCode}");
                }

                // Parse profile data
                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    profileData = JsonSerializer.Deserialize<FmpProfileResponse[]>(profileJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else
                {
                    _logger.LogWarning($"Failed to get profile data for {symbol}: {profileResponse.StatusCode}");
                }

                // Combine data to create Stock object
                if (quoteData != null && quoteData.Length > 0)
                {
                    var quote = quoteData[0];
                    var profile = profileData?.FirstOrDefault();

                    var stock = new Stock
                    {
                        Symbol = quote.Symbol.ToUpper(),
                        CompanyName = profile?.CompanyName ?? quote.Name ?? quote.Symbol,
                        Purchase = quote.Price,
                        CurrentPrice = quote.Price,
                        MarketCap = profile?.MktCap ?? quote.MarketCap,
                        LastDiv = profile?.LastDiv ?? 0,
                        Industry = profile?.Industry ?? "Unknown",
                        Country = profile?.Country ?? "US",
                        PERatio = quote.Pe > 0 ? quote.Pe : null,
                        DividendYield = profile?.LastDiv != null && quote.Price > 0
                            ? (profile.LastDiv / quote.Price) * 100
                            : null,
                        LastUpdated = DateTime.UtcNow,
                        IsActive = true
                    };

                    _logger.LogInformation($"Successfully fetched FMP data for {symbol}: {stock.CompanyName} - ${stock.CurrentPrice}");
                    return stock;
                }

                _logger.LogWarning($"No valid data found for symbol: {symbol}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stock data for {symbol}");
                return null;
            }
        }

        public async Task<bool> PopulateStocksFromSymbolListAsync(List<string> symbols)
        {
            var successCount = 0;
            var totalCount = symbols.Count;

            _logger.LogInformation($"Starting to populate {totalCount} stocks from FMP API");

            foreach (var symbol in symbols)
            {
                try
                {
                    // Check if stock already exists
                    var existingStock = await _context.Stocks
                        .FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpper());

                    if (existingStock != null)
                    {
                        _logger.LogDebug($"Stock {symbol} already exists, skipping");
                        continue;
                    }

                    var stockData = await GetStockDataFromFmpAsync(symbol);
                    if (stockData != null)
                    {
                        await _context.Stocks.AddAsync(stockData);
                        await _context.SaveChangesAsync();
                        successCount++;
                        _logger.LogInformation($"Successfully added stock: {symbol} - {stockData.CompanyName}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to get FMP data for {symbol}");
                    }

                    // FMP has rate limits, add delay
                    await Task.Delay(250); // FMP free plan: 250 requests/day, paid plans much higher
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing stock {symbol}");
                }
            }

            _logger.LogInformation($"Successfully populated {successCount} out of {totalCount} stocks using FMP API");
            return successCount > 0;
        }

        public async Task SeedPopularStocksAsync()
        {
            var popularSymbols = await GetPopularStockSymbolsAsync();
            await PopulateStocksFromSymbolListAsync(popularSymbols);
        }

        public async Task<bool> UpdateStockFundamentalsAsync(string symbol)
        {
            try
            {
                var existingStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpper());

                if (existingStock == null)
                {
                    _logger.LogWarning($"Stock {symbol} not found in database");
                    return false;
                }

                var updatedData = await GetStockDataFromFmpAsync(symbol);
                if (updatedData == null)
                {
                    _logger.LogWarning($"Could not fetch updated FMP data for {symbol}");
                    return false;
                }

                // Update existing stock with new data
                existingStock.CompanyName = updatedData.CompanyName;
                existingStock.CurrentPrice = updatedData.CurrentPrice;
                existingStock.MarketCap = updatedData.MarketCap;
                existingStock.LastDiv = updatedData.LastDiv;
                existingStock.Industry = updatedData.Industry;
                existingStock.Country = updatedData.Country;
                existingStock.PERatio = updatedData.PERatio;
                existingStock.DividendYield = updatedData.DividendYield;
                existingStock.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated fundamentals for {symbol} using FMP API");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating fundamentals for {symbol}");
                return false;
            }
        }
    }
}
