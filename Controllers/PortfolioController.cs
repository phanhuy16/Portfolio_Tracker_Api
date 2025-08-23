using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.DTOs.Portfolio;
using server.Extensions;
using server.Interfaces;
using server.Mappers;
using server.Models;
using server.Services;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace server.Controllers
{
    [Route("api/client/portfolio")]
    [ApiController]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IStockPriceService _stockPriceService;

        public PortfolioController(UserManager<AppUser> userManager,
            IStockRepository stockRepository,
            IStockPriceService stockPriceService,
            IPortfolioRepository portfolioRepository)
        {
            _userManager = userManager;
            _stockRepository = stockRepository;
            _portfolioRepository = portfolioRepository;
            _stockPriceService = stockPriceService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserPortfolio()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var userPortfolio = await _portfolioRepository.GetUserPortfoliosAsync(user!.Id);

            // AUTO - UPDATE PRICES khi user xem portfolio
            await UpdatePortfolioPricesIfNeeded(userPortfolio);

            var portfolioDtos = userPortfolio.Select(p => p.ToPortfolioDto()).ToList();

            return Ok(portfolioDtos);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetPortfolioSummary()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            // Lấy portfolios và update prices
            var userPortfolios = await _portfolioRepository.GetUserPortfoliosAsync(user!.Id);
            await UpdatePortfolioPricesIfNeeded(userPortfolios);

            var summary = await _portfolioRepository.GetPortfolioSummaryAsync(user!.Id);

            return Ok(summary);
        }

        // Refresh prices manually
        [HttpPost("refresh-prices")]
        public async Task<IActionResult> RefreshPortfolioPrices()
        {
            var username = User.GetUsername();
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);
            var userPortfolio = await _portfolioRepository.GetUserPortfoliosAsync(user!.Id);

            var updateCount = 0;
            foreach (var portfolio in userPortfolio)
            {
                var currentPrice = await _stockPriceService.GetCurrentPriceAsync(portfolio.Stock.Symbol);
                if (currentPrice.HasValue)
                {
                    portfolio.CurrentPrice = currentPrice.Value;
                    portfolio.LastUpdated = DateTime.UtcNow;
                    updateCount++;
                }
            }

            if (updateCount > 0)
            {
                await _portfolioRepository.SaveChangesAsync(); // Bạn cần thêm method này
            }

            return Ok(new
            {
                Message = $"Updated prices for {updateCount} holdings",
                UpdatedCount = updateCount,
                TotalHoldings = userPortfolio.Count
            });
        }


        [HttpGet("{stockId:int}")]
        public async Task<IActionResult> GetPortfolioByStock([FromRoute] int stockId)
        {
            var username = User.GetUsername();
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return Unauthorized("User not found.");

            var portfolio = await _portfolioRepository.GetUserPortfolioAsync(user.Id, stockId);
            if (portfolio == null)
                return NotFound($"No portfolio found for stock with ID {stockId}");

            // Update price for this specific holding
            var currentPrice = await _stockPriceService.GetCurrentPriceAsync(portfolio.Stock.Symbol);
            if (currentPrice.HasValue)
            {
                portfolio.CurrentPrice = currentPrice.Value;
                portfolio.LastUpdated = DateTime.UtcNow;
                await _portfolioRepository.SaveChangesAsync();
            }

            return Ok(portfolio.ToPortfolioDto());

        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolio([FromBody] CreatePortfolioDto createPortfolioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var stock = await _stockRepository.GetByIdAsync(createPortfolioDto.StockId);

            if (stock == null) return BadRequest("Stock not found");

            var existingPortfolio = await _portfolioRepository.GetUserPortfolioAsync(user!.Id, createPortfolioDto.StockId);

            if (existingPortfolio != null)
            {
                return BadRequest("Stock already exists in portfolio");
            }

            var portfolioModel = createPortfolioDto.ToPortfolioFromCreate(user.Id);

            // GET CURRENT PRICE khi tạo portfolio
            var currentPrice = await _stockPriceService.GetCurrentPriceAsync(stock.Symbol);
            if (currentPrice.HasValue)
            {
                portfolioModel.CurrentPrice = currentPrice.Value;
            }

            await _portfolioRepository.CreateAsync(portfolioModel);
            return CreatedAtAction(nameof(GetUserPortfolio), null, portfolioModel.ToPortfolioDto());
        }

        [HttpPut("{stockId:int}")]
        public async Task<IActionResult> UpdatePortfolio([FromRoute] int stockId, [FromBody] UpdatePortfolioDto updatePortfolioDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.FindByNameAsync(username);

            var updatedPortfolio = await _portfolioRepository.UpdateAsync(user!.Id, stockId, updatePortfolioDto);

            if (updatedPortfolio == null)
            {
                return NotFound($"No portfolio found for the stock with ID {stockId}.");
            }

            return Ok(updatedPortfolio.ToPortfolioDto());
        }

        [HttpDelete("{stockId:int}")]
        public async Task<IActionResult> DeletePortfolio([FromRoute] int stockId)
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var portfolio = await _portfolioRepository.DeleteAsync(appUser!.Id, stockId);

            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            return Ok(new
            {
                Message = $"Portfolio with Stock {stockId} has been deleted successfully",
            });
        }

        private async Task UpdatePortfolioPricesIfNeeded(List<Portfolio> portfolios)
        {
            const int UPDATE_INTERVAL_MINUTES = 15;
            var needsUpdate = portfolios.Any(p =>
                p.LastUpdated < DateTime.UtcNow.AddMinutes(-UPDATE_INTERVAL_MINUTES));

            if (needsUpdate)
            {
                foreach (var portfolio in portfolios)
                {
                    if (portfolio.LastUpdated < DateTime.UtcNow.AddMinutes(-UPDATE_INTERVAL_MINUTES))
                    {
                        var currentPrice = await _stockPriceService.GetCurrentPriceAsync(portfolio.Stock.Symbol);
                        if (currentPrice.HasValue)
                        {
                            portfolio.CurrentPrice = currentPrice.Value;
                            portfolio.LastUpdated = DateTime.UtcNow;
                        }
                    }
                }
                await _portfolioRepository.SaveChangesAsync();
            }
        }
    }
}
