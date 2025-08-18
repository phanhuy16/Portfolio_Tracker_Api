namespace server.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendHtmlEmailAsync(string email, string subject, string htmlMessage);
        Task SendPortfolioSummaryAsync(string email, object portfolioData);
        Task SendAlertAsync(string email, string stockSymbol, decimal triggerPrice, decimal currentPrice);
        Task SendWelcomeEmailAsync(string email, string firstName, string lastName);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}
