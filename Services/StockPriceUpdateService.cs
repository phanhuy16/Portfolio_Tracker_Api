using Microsoft.Extensions.Hosting;
using server.Interfaces;

namespace server.Services
{
    public class StockPriceUpdateService : BackgroundService
    {
        private readonly ILogger<StockPriceUpdateService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;

        public StockPriceUpdateService(
            ILogger<StockPriceUpdateService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting stock price update at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var stockPriceService = scope.ServiceProvider.GetRequiredService<IStockPriceService>();
                        var intervalMinutesStr = _configuration["BackgroundServices:StockPriceUpdateIntervalMinutes"];
                        if (!double.TryParse(intervalMinutesStr, out var intervalMinutes))
                        {
                            intervalMinutes = 15; // fallback
                        }
                        await stockPriceService.UpdateAllStockPricesAsync();
                        await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                    }

                    _logger.LogInformation("Stock price update completed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during stock price update");
                }
            }
        }
    }
}
