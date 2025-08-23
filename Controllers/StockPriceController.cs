using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Fmp;
using server.Interfaces;

namespace server.Controllers
{
    [Route("api/admin/stock-prices")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StockPriceController : ControllerBase
    {
        private readonly IStockPriceService _stockPriceService;
        private readonly ILogger<StockPriceController> _logger;

        public StockPriceController(IStockPriceService stockPriceService, ILogger<StockPriceController> logger)
        {
            _stockPriceService = stockPriceService;
            _logger = logger;
        }

        [HttpGet("current/{symbol}")]
        public async Task<ActionResult> GetCurrentPrice(string symbol)
        {
            var price = await _stockPriceService.GetCurrentPriceAsync(symbol);

            if (!price.HasValue)
                return NotFound($"Price not found for symbol: {symbol}");

            return Ok(new { Symbol = symbol.ToUpper(), Price = price.Value, UpdatedAt = DateTime.UtcNow });
        }

        [HttpPost("force-real-update/{symbol}")]
        public async Task<ActionResult> ForceRealDataUpdate(string symbol)
        {
            try
            {
                var success = await _stockPriceService.ForceUpdateWithRealDataAsync(symbol);

                if (success)
                {
                    return Ok(new
                    {
                        Message = $"Successfully force updated {symbol} with REAL Yahoo Finance data",
                        UpdatedAt = DateTime.UtcNow,
                        DataType = "REAL"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Message = $"Failed to get real data for {symbol}",
                        Reason = "Yahoo API unavailable or no data found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error force updating {symbol}");
                return StatusCode(500, $"Internal error updating {symbol}");
            }
        }

        [HttpPost("update-all")]
        public async Task<ActionResult> UpdateAllStockPrices()
        {
            try
            {
                await _stockPriceService.UpdateAllStockPricesAsync();
                return Ok(new { Message = "All stock prices updated successfully", UpdatedAt = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update all stock prices");
                return BadRequest("Failed to update stock prices");
            }
        }

        [HttpGet("historical/{stockId}")]
        public async Task<ActionResult> GetHistoricalPrices(int stockId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            var historicalPrices = await _stockPriceService.GetHistoricalPricesAsync(stockId, start, end);

            return Ok(new
            {
                StockId = stockId,
                StartDate = start,
                EndDate = end,
                Count = historicalPrices.Count,
                Prices = historicalPrices
            });
        }

        [HttpPost("batch-update")]
        public async Task<ActionResult> BatchUpdatePrices([FromBody] List<string> symbols)
        {
            try
            {
                if (symbols == null || !symbols.Any())
                {
                    return BadRequest(new { Message = "No symbols provided for batch update" });
                }

                var success = await _stockPriceService.BatchUpdatePricesAsync(symbols);
                if (success)
                {
                    return Ok(new
                    {
                        Message = $"Successfully batch updated prices for {symbols.Count} symbols",
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Batch update failed for provided symbols");
                    return BadRequest(new
                    {
                        Message = "Failed to batch update prices",
                        Reason = "No valid data updated or API error"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch price update");
                return StatusCode(500, new { Message = "Internal error during batch price update" });
            }
        }

        [HttpPost("bulk-historical")]
        public async Task<ActionResult> BulkUpdateHistoricalData([FromBody] BulkUpdateRequest request)
        {
            try
            {
                if (request == null || request.Symbols == null || !request.Symbols.Any())
                {
                    return BadRequest(new { Message = "No symbols provided for bulk historical update" });
                }

                if (request.FromDate > request.ToDate)
                {
                    return BadRequest(new { Message = "FromDate cannot be later than ToDate" });
                }

                var success = await _stockPriceService.BulkUpdateHistoricalDataAsync(request.Symbols, request.FromDate, request.ToDate);
                if (success)
                {
                    return Ok(new
                    {
                        Message = $"Successfully updated historical data for {request.Symbols.Count} symbols",
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning("Bulk historical data update failed for provided symbols");
                    return BadRequest(new
                    {
                        Message = "Failed to update historical data",
                        Reason = "No valid data updated or API error"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk historical data update");
                return StatusCode(500, new { Message = "Internal error during bulk historical data update" });
            }
        }
    }
}
