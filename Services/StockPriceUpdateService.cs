using server.Interfaces;

namespace server.Services
{
    public class StockPriceUpdateService : BackgroundService
    {
        private readonly ILogger<StockPriceUpdateService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Update every 5 minutes

        public StockPriceUpdateService(
            ILogger<StockPriceUpdateService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
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
                        await stockPriceService.UpdateAllStockPricesAsync();
                    }

                    _logger.LogInformation("Stock price update completed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during stock price update");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
