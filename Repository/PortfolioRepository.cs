using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Portfolio;
using server.Interfaces;
using server.Models;

namespace server.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _context;
        public PortfolioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            try
            {
                await _context.Portfolios.AddAsync(portfolio);
                await _context.SaveChangesAsync();
                return portfolio;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("An error occurred while creating the portfolio.", ex);
            }
        }

        public async Task<Portfolio?> DeleteAsync(string userId, int stockId)
        {
            var portfolio = await _context.Portfolios
                .FirstOrDefaultAsync(p => p.UserId == userId && p.StockId == stockId);

            if (portfolio == null)
            {
                return null!;
            }

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId)
        {
            try
            {
                var portfolios = await GetUserPortfoliosAsync(userId);

                var summary = new PortfolioSummaryDto
                {
                    TotalStocks = portfolios.Count,
                    TotalInvestment = portfolios.Sum(p => p.Quantity * p.PurchasePrice),
                    TotalCurrentValue = portfolios.Where(p => p.CurrentPrice.HasValue)
                        .Sum(p => p.CurrentValue),
                    Holdings = portfolios.Select(p => new PortfolioDto
                    {
                        Id = p.Id,
                        Symbol = p.Stock.Symbol,
                        CompanyName = p.Stock.CompanyName,
                        Quantity = p.Quantity,
                        PurchasePrice = p.PurchasePrice,
                        PurchaseDate = p.PurchaseDate,
                        CurrentPrice = p.CurrentPrice,
                        TotalCost = p.TotalCost,
                        CurrentValue = p.CurrentValue,
                        ProfitLoss = p.ProfitLoss,
                        ProfitLossPercentage = p.ProfitLossPercentage
                    }).ToList()
                };

                if (summary.TotalCurrentValue.HasValue)
                {
                    summary.TotalProfitLoss = summary.TotalCurrentValue.Value - summary.TotalInvestment;
                    summary.TotalProfitLossPercentage = summary.TotalInvestment > 0
                        ? (summary.TotalProfitLoss.Value / summary.TotalInvestment) * 100
                        : null;
                }

                var performersWithData = summary.Holdings
                    .Where(x => x.ProfitLossPercentage.HasValue)
                    .ToList();

                summary.TopPerformers = performersWithData
                    .OrderByDescending(x => x.ProfitLossPercentage)
                    .Take(5)
                    .Select(summary => new TopPerformerDto
                    {
                        Symbol = summary.Symbol,
                        CompanyName = summary.CompanyName,
                        ProfitLoss = summary.ProfitLoss,
                        ProfitLossPercentage = summary.ProfitLossPercentage
                    }).ToList();

                summary.WorstPerformers = performersWithData
                    .OrderBy(x => x.ProfitLossPercentage)
                    .Take(5)
                    .Select(summary => new TopPerformerDto
                    {
                        Symbol = summary.Symbol,
                        CompanyName = summary.CompanyName,
                        ProfitLoss = summary.ProfitLoss,
                        ProfitLossPercentage = summary.ProfitLossPercentage
                    }).ToList();

                return summary;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("An error occurred while retrieving the portfolio summary.", ex);
            }
        }

        public async Task<Portfolio?> GetUserPortfolioAsync(string userId, int stockId)
        {
            try
            {
                return await _context.Portfolios
                    .Where(p => p.UserId == userId && p.StockId == stockId)
                    .Include(p => p.AppUser)
                    .ThenInclude(x => x.Comments)
                    .Include(p => p.Stock)
                    .ThenInclude(x => x.Transactions)
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.StockId == stockId);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("An error occurred while retrieving the user portfolio.", ex);
            }
        }

        public async Task<List<Portfolio>> GetUserPortfoliosAsync(string userId)
        {
            try
            {
                return await _context.Portfolios
                    .Where(p => p.UserId == userId)
                    .Include(p => p.AppUser)
                    .ThenInclude(x => x.Comments)
                    .Include(p => p.Stock)
                    .ThenInclude(x => x.Transactions)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("An error occurred while retrieving user portfolios.", ex);
            }
        }

        public async Task<Portfolio?> UpdateAsync(string userId, int stockId, UpdatePortfolioDto portfolioDto)
        {
            try
            {
                var existingPortfolio = await _context.Portfolios
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.StockId == stockId);

                if (existingPortfolio == null)
                {
                    return null;
                }


                // Update the properties of the existing portfolio
                existingPortfolio.Quantity = portfolioDto.Quantity;
                existingPortfolio.PurchasePrice = portfolioDto.PurchasePrice;
                existingPortfolio.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();
                return existingPortfolio;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("An error occurred while updating the portfolio.", ex);
            }
        }
    }
}
