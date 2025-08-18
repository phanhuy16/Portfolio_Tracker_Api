using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs.Stocks;
using server.Helpers;
using server.Interfaces;
using server.Models;

namespace server.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _context;
        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            try
            {
                var stocks = _context.Stocks
                    .Include(x => x.Transactions)
                    .Include(x => x.Comments)
                    .ThenInclude(x => x.AppUser)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query.searchTerm))
                {
                    stocks = stocks.Where(s => (s.CompanyName.Contains(query.searchTerm.ToLower()) ||
                                  s.Symbol.Contains(query.searchTerm.ToLower())) && s.IsActive);
                }

                if (!string.IsNullOrEmpty(query.SortBy))
                {
                    if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                    {
                        stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
                    }
                    else if (query.SortBy.Equals("CompanyName", StringComparison.OrdinalIgnoreCase))
                    {
                        stocks = query.IsDescending ? stocks.OrderByDescending(s => s.CompanyName) : stocks.OrderBy(s => s.CompanyName);
                    }
                    else if (query.SortBy.Equals("MarketCap", StringComparison.OrdinalIgnoreCase))
                    {
                        stocks = query.IsDescending ? stocks.OrderByDescending(s => s.MarketCap) : stocks.OrderBy(s => s.MarketCap);
                    }

                }
                var skip = (query.PageNumber - 1) * query.PageSize;
                stocks = stocks.Skip(skip).Take(query.PageSize);

                return await stocks.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException("An error occurred while retrieving stocks.", ex);
            }
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            try
            {
                var stock = await _context.Stocks
                    .Include(s => s.Comments)
                        .ThenInclude(c => c.AppUser)
                     .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
                return stock ?? null!;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException($"An error occurred while retrieving stock with ID {id}.", ex);
            }
        }

        public async Task<Stock> CreateAsync(Stock stock)
        {
            try
            {
                stock.Symbol = stock.Symbol.ToUpper();
                await _context.Stocks.AddAsync(stock);
                await _context.SaveChangesAsync();
                return stock;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException("An error occurred while creating the stock.", ex);
            }
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            try
            {
                var stockModel = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);
                if (stockModel == null) return null;

                // Soft delete
                stockModel.IsActive = false;
                await _context.SaveChangesAsync();
                return stockModel;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException($"An error occurred while deleting stock with ID {id}.", ex);
            }
        }

        public Task<bool> IsStockExistsAsync(int id)
        {
            return _context.Stocks.AnyAsync(s => s.Id == id && s.IsActive);
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            try
            {
                return await _context.Stocks
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Symbol == symbol.ToUpper() && s.IsActive);
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException($"An error occurred while retrieving stock with symbol {symbol}.", ex);
            }
        }

        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            try
            {
                var existingStock = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);
                if (existingStock == null) return null;

                existingStock.CompanyName = stockDto.CompanyName;
                existingStock.Purchase = stockDto.Purchase;
                existingStock.MarketCap = stockDto.MarketCap;
                existingStock.LastDiv = stockDto.LastDiv;
                existingStock.Industry = stockDto.Industry;
                existingStock.Country = stockDto.Country;
                existingStock.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();
                return existingStock;
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException($"An error occurred while updating stock with ID {id}.", ex);
            }
        }

        public Task<List<Stock>> GetStocksByIndustryAsync(string industry)
        {
            try
            {
                return _context.Stocks
                    .Where(s => s.Industry.ToLower() == industry.ToLower() && s.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                throw new InvalidOperationException($"An error occurred while retrieving stocks by industry {industry}.", ex);
            }
        }
    }
}
