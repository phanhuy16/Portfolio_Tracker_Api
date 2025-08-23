using server.DTOs.Portfolio;
using server.Models;

namespace server.Interfaces
{
    public interface IPortfolioRepository
    {
        Task<List<Portfolio>> GetUserPortfoliosAsync(string userId);
        Task<Portfolio?> GetUserPortfolioAsync(string userId, int stockId);
        Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(string userId);
        Task<Portfolio> CreateAsync(Portfolio portfolio);
        Task<Portfolio?> UpdateAsync(string userId, int stockId, UpdatePortfolioDto portfolioDto);
        Task<Portfolio?> DeleteAsync(string userId, int stockId);
        Task SaveChangesAsync();
    }
}
