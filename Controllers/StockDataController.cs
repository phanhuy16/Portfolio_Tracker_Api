using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Interfaces;

namespace server.Controllers
{
    [Route("api/admin/stock-data")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class StockDataController : ControllerBase
    {
        private readonly IStockDataService _stockDataService;
        private readonly ILogger<StockDataController> _logger;

        public StockDataController(IStockDataService stockDataService, ILogger<StockDataController> logger)
        {
            _stockDataService = stockDataService;
            _logger = logger;
        }

        [HttpPost("seed-popular")]
        public async Task<ActionResult> SeedPopularStocks()
        {
            try
            {
                await _stockDataService.SeedPopularStocksAsync();
                return Ok(new { Message = "Successfully seeded popular stocks from Yahoo Finance" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed popular stocks");
                return BadRequest("Failed to seed stocks");
            }
        }

        [HttpPost("add-stock/{symbol}")]
        public async Task<ActionResult> AddStockBySymbol(string symbol)
        {
            try
            {
                var symbols = new List<string> { symbol };
                var success = await _stockDataService.PopulateStocksFromSymbolListAsync(symbols);

                if (success)
                {
                    return Ok(new { Message = $"Successfully added stock: {symbol}" });
                }
                else
                {
                    return BadRequest($"Failed to add stock: {symbol}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add stock {symbol}");
                return BadRequest($"Failed to add stock: {symbol}");
            }
        }

        [HttpPost("add-multiple")]
        public async Task<ActionResult> AddMultipleStocks([FromBody] List<string> symbols)
        {
            try
            {
                if (!symbols.Any())
                {
                    return BadRequest("Symbol list cannot be empty");
                }

                var success = await _stockDataService.PopulateStocksFromSymbolListAsync(symbols);
                return Ok(new
                {
                    Message = $"Processing completed for {symbols.Count} symbols",
                    Success = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add multiple stocks");
                return BadRequest("Failed to add stocks");
            }
        }

        [HttpPut("update-fundamentals/{symbol}")]
        public async Task<ActionResult> UpdateStockFundamentals(string symbol)
        {
            try
            {
                var success = await _stockDataService.UpdateStockFundamentalsAsync(symbol);

                if (success)
                {
                    return Ok(new { Message = $"Successfully updated fundamentals for: {symbol}" });
                }
                else
                {
                    return BadRequest($"Failed to update fundamentals for: {symbol}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update fundamentals for {symbol}");
                return BadRequest($"Failed to update fundamentals: {symbol}");
            }
        }

        [HttpGet("stock-info/{symbol}")]
        public async Task<ActionResult> GetStockInfoFromYahoo(string symbol)
        {
            try
            {
                var stockData = await _stockDataService.GetStockDataFromFmpAsync(symbol);

                if (stockData != null)
                {
                    return Ok(stockData);
                }
                else
                {
                    return NotFound($"No data found for symbol: {symbol}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get stock info for {symbol}");
                return BadRequest($"Failed to get stock info: {symbol}");
            }
        }
    }
}
