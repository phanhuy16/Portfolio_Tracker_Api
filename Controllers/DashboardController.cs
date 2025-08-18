using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using server.Extensions;
using server.Interfaces;
using server.Models;

namespace server.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICommentRepository _commentRepository;

        public DashboardController(
            UserManager<AppUser> userManager,
            IPortfolioRepository portfolioRepository,
            ITransactionRepository transactionRepository,
            ICommentRepository commentRepository)
        {
            _userManager = userManager;
            _portfolioRepository = portfolioRepository;
            _transactionRepository = transactionRepository;
            _commentRepository = commentRepository;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            // Get portfolio summary
            var portfolioSummary = await _portfolioRepository.GetPortfolioSummaryAsync(appUser!.Id);

            // Get recent transactions
            var recentTransactions = await _transactionRepository.GetUserTransactionsAsync(appUser.Id);
            var last5Transactions = recentTransactions.Take(5).ToList();

            // Get user stats
            var totalTransactions = recentTransactions.Count;
            var totalComments = await _commentRepository.GetAllAsync();
            var userComments = totalComments.Where(c => c.UserId == appUser.Id).Count();

            var dashboardData = new
            {
                Portfolio = portfolioSummary,
                RecentTransactions = last5Transactions.Select(t => new
                {
                    t.Id,
                    Symbol = t.Stock?.Symbol,
                    Type = t.TransactionType.ToString(),
                    t.Quantity,
                    t.Price,
                    t.TransactionDate,
                    TotalAmount = t.TotalAmount
                }),
                UserStats = new
                {
                    TotalStocks = portfolioSummary.TotalStocks,
                    TotalTransactions = totalTransactions,
                    TotalComments = userComments,
                    MemberSince = appUser.DateJoined.ToString("MMMM yyyy")
                },
                Performance = new
                {
                    TotalInvestment = portfolioSummary.TotalInvestment,
                    CurrentValue = portfolioSummary.TotalCurrentValue,
                    ProfitLoss = portfolioSummary.TotalProfitLoss,
                    ProfitLossPercentage = portfolioSummary.TotalProfitLossPercentage,
                    TopPerformers = portfolioSummary.TopPerformers.Take(3),
                    WorstPerformers = portfolioSummary.WorstPerformers.Take(3)
                }
            };

            return Ok(dashboardData);
        }

        [HttpGet("portfolio-allocation")]
        public async Task<IActionResult> GetPortfolioAllocation()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var portfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser!.Id);

            var allocation = portfolios
                .Where(p => p.CurrentValue.HasValue)
                .GroupBy(p => p.Stock.Industry)
                .Select(g => new
                {
                    Industry = g.Key,
                    Value = g.Sum(p => p.CurrentValue!.Value),
                    Percentage = 0.0m, // Will be calculated
                    StockCount = g.Count()
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            var totalValue = allocation.Sum(a => a.Value);

            if (totalValue > 0)
            {
                allocation = allocation.Select(a => new
                {
                    a.Industry,
                    a.Value,
                    Percentage = Math.Round((a.Value / totalValue) * 100, 2),
                    a.StockCount
                }).ToList();
            }

            return Ok(allocation);
        }

        [HttpGet("performance-chart")]
        public async Task<IActionResult> GetPerformanceChart([FromQuery] int days = 30)
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-days);

            var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(appUser!.Id, startDate, endDate);

            // Group transactions by date and calculate daily portfolio value
            var dailyData = new List<object>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dayTransactions = transactions.Where(t => t.TransactionDate.Date == currentDate).ToList();
                var dayValue = dayTransactions.Sum(t =>
                    t.TransactionType == TransactionType.Buy ? t.TotalAmount : -t.TotalAmount);

                dailyData.Add(new
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    Value = dayValue,
                    TransactionCount = dayTransactions.Count
                });

                currentDate = currentDate.AddDays(1);
            }

            return Ok(new
            {
                Period = $"{days} days",
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                Data = dailyData
            });
        }

        [HttpGet("market-movers")]
        public async Task<IActionResult> GetMarketMovers()
        {
            var username = User.GetUsername();

            if (string.IsNullOrEmpty(username))
                return Unauthorized("User is not authenticated.");

            var appUser = await _userManager.FindByNameAsync(username);

            var portfolios = await _portfolioRepository.GetUserPortfoliosAsync(appUser!.Id);

            var movers = portfolios
                .Where(p => p.ProfitLossPercentage.HasValue)
                .Select(p => new
                {
                    Symbol = p.Stock.Symbol,
                    CompanyName = p.Stock.CompanyName,
                    CurrentPrice = p.CurrentPrice,
                    PurchasePrice = p.PurchasePrice,
                    Change = p.ProfitLoss,
                    ChangePercentage = p.ProfitLossPercentage,
                    Volume = p.Quantity,
                    Industry = p.Stock.Industry
                })
                .OrderByDescending(x => Math.Abs(x.ChangePercentage ?? 0))
                .Take(10)
                .ToList();

            return Ok(new
            {
                TopMovers = movers.Where(m => (m.ChangePercentage ?? 0) > 0).Take(5),
                TopLosers = movers.Where(m => (m.ChangePercentage ?? 0) < 0).Take(5)
            });
        }
    }
}
