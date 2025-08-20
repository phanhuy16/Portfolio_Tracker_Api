using server.Interfaces;
using System.Net;
using System.Net.Mail;

namespace server.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                await SendEmailWithSmtp(email, subject, message, isHtml: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }

        public async Task SendPortfolioSummaryAsync(string email, object portfolioData)
        {
            var subject = "📈 Your Portfolio Summary - " + DateTime.Now.ToString("MMMM dd, yyyy");
            var htmlTemplate = GetPortfolioSummaryTemplate(portfolioData);
            await SendHtmlEmailAsync(email, subject, htmlTemplate);
        }

        public async Task SendAlertAsync(string email, string stockSymbol, decimal triggerPrice, decimal currentPrice)
        {
            var isPositiveAlert = currentPrice >= triggerPrice;
            var emoji = isPositiveAlert ? "🚀" : "⚠️";
            var subject = $"{emoji} Price Alert: {stockSymbol}";
            var htmlTemplate = GetAlertEmailTemplate(stockSymbol, triggerPrice, currentPrice, isPositiveAlert);
            await SendHtmlEmailAsync(email, subject, htmlTemplate);
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            var subject = "🎉 Welcome to Portfolio Tracker!";
            var htmlTemplate = GetWelcomeEmailTemplate(email);
            await SendHtmlEmailAsync(email, subject, htmlTemplate);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "🔐 Reset Your Portfolio Tracker Password";
            var htmlTemplate = GetPasswordResetTemplate(resetLink);
            await SendHtmlEmailAsync(email, subject, htmlTemplate);
        }

        public async Task SendHtmlEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                await SendEmailWithSmtp(email, subject, htmlMessage, isHtml: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send HTML email to {Email}", email);
                throw;
            }
        }

        private async Task SendEmailWithSmtp(string email, string subject, string message, bool isHtml)
        {
            // Validation
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email address cannot be null or empty");

            if (string.IsNullOrEmpty(_configuration["Email:SmtpServer"]))
                throw new InvalidOperationException("SMTP server configuration is missing");

            if (string.IsNullOrEmpty(_configuration["Email:FromEmail"]))
                throw new InvalidOperationException("From email configuration is missing");

            using var client = new SmtpClient(_configuration["Email:SmtpServer"],
                int.Parse(_configuration["Email:SmtpPort"] ?? "587"));

            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);
            client.EnableSsl = true;

            // Set timeout (optional)
            client.Timeout = 30000; // 30 seconds

            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_configuration["Email:FromEmail"]!, _configuration["Email:FromName"] ?? "Portfolio Tracker");
            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = isHtml;

            // Set encoding
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email} via SMTP", email);
        }

        private string GetPortfolioSummaryTemplate(object portfolioData)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ 
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                            line-height: 1.6; 
                            color: #333; 
                            margin: 0; 
                            padding: 0; 
                            background-color: #f4f4f4; 
                        }}
                        .container {{ 
                            max-width: 600px; 
                            margin: 0 auto; 
                            background-color: white; 
                            box-shadow: 0 0 20px rgba(0,0,0,0.1); 
                        }}
                        .header {{ 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; 
                            padding: 30px; 
                            text-align: center; 
                            border-radius: 10px 10px 0 0; 
                        }}
                        .header h1 {{ margin: 0; font-size: 28px; }}
                        .content {{ 
                            background: #f8f9fa; 
                            padding: 30px; 
                            border-radius: 0 0 10px 10px; 
                        }}
                        .stats {{ 
                            display: flex; 
                            justify-content: space-between; 
                            margin: 20px 0; 
                            flex-wrap: wrap; 
                            gap: 15px; 
                        }}
                        .stat-item {{ 
                            text-align: center; 
                            padding: 20px; 
                            background: white; 
                            border-radius: 8px; 
                            box-shadow: 0 2px 8px rgba(0,0,0,0.1); 
                            flex: 1; 
                            min-width: 150px; 
                        }}
                        .stat-item h3 {{ margin: 0 0 10px 0; font-size: 14px; color: #666; }}
                        .positive {{ color: #28a745; }}
                        .negative {{ color: #dc3545; }}
                        .cta-button {{ 
                            background: #667eea; 
                            color: white; 
                            padding: 15px 30px; 
                            text-decoration: none; 
                            border-radius: 8px; 
                            display: inline-block; 
                            font-weight: bold; 
                            transition: background-color 0.3s; 
                        }}
                        .cta-button:hover {{ background: #5a6fd8; }}
                        .footer {{ 
                            text-align: center; 
                            margin-top: 30px; 
                            font-size: 12px; 
                            color: #666; 
                            padding: 20px; 
                            border-top: 1px solid #eee; 
                        }}
                        @media (max-width: 600px) {{
                            .stats {{ flex-direction: column; }}
                            .container {{ margin: 0 10px; }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>📈 Tổng quan Danh mục Đầu tư</h1>
                            <p>Báo cáo đầu tư của bạn ngày {DateTime.Now:dd/MM/yyyy}</p>
                        </div>
                        <div class='content'>
                            <h2>Hiệu suất Danh mục</h2>
                            <div class='stats'>
                                <div class='stat-item'>
                                    <h3>Tổng Đầu tư</h3>
                                    <p style='font-size: 24px; font-weight: bold; margin: 0;'>$0.00</p>
                                </div>
                                <div class='stat-item'>
                                    <h3>Giá trị Hiện tại</h3>
                                    <p style='font-size: 24px; font-weight: bold; margin: 0;'>$0.00</p>
                                </div>
                                <div class='stat-item'>
                                    <h3>Lãi/Lỗ</h3>
                                    <p style='font-size: 24px; font-weight: bold; margin: 0;' class='positive'>+$0.00 (+0.00%)</p>
                                </div>
                            </div>
                            <p>Truy cập bảng điều khiển của bạn để xem phân tích chi tiết và thông tin chi tiết.</p>
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='http://localhost:3000/dashboard' class='cta-button'>Xem Bảng điều khiển</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>© 2025 Portfolio Tracker. Tất cả quyền được bảo lưu.</p>
                            <p>Nếu bạn không muốn nhận email này nữa, <a href='#'>bấm vào đây để hủy đăng ký</a></p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetAlertEmailTemplate(string stockSymbol, decimal triggerPrice, decimal currentPrice, bool isPositive)
        {
            var statusColor = isPositive ? "#28a745" : "#dc3545";
            var statusText = isPositive ? "Đạt Mục tiêu!" : "Cảnh báo Giá";
            var changeText = isPositive ? "trên" : "dưới";

            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ 
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                            line-height: 1.6; 
                            color: #333; 
                            margin: 0; 
                            padding: 0; 
                            background-color: #f4f4f4; 
                        }}
                        .container {{ 
                            max-width: 600px; 
                            margin: 0 auto; 
                            background-color: white; 
                            box-shadow: 0 0 20px rgba(0,0,0,0.1); 
                        }}
                        .header {{ 
                            background: {statusColor}; 
                            color: white; 
                            padding: 30px; 
                            text-align: center; 
                            border-radius: 10px 10px 0 0; 
                        }}
                        .header h1 {{ margin: 0; font-size: 28px; }}
                        .header h2 {{ margin: 10px 0 0 0; font-size: 32px; font-weight: bold; }}
                        .content {{ 
                            background: #f8f9fa; 
                            padding: 30px; 
                            border-radius: 0 0 10px 10px; 
                        }}
                        .alert-info {{ 
                            background: white; 
                            padding: 25px; 
                            border-radius: 8px; 
                            margin: 20px 0; 
                            text-align: center; 
                            box-shadow: 0 2px 8px rgba(0,0,0,0.1); 
                        }}
                        .price-info {{ 
                            display: flex; 
                            justify-content: space-around; 
                            margin: 20px 0; 
                            flex-wrap: wrap; 
                        }}
                        .price-item {{ 
                            text-align: center; 
                            padding: 15px; 
                            flex: 1; 
                            min-width: 120px; 
                        }}
                        .price-item strong {{ display: block; margin-bottom: 5px; }}
                        .cta-button {{ 
                            background: {statusColor}; 
                            color: white; 
                            padding: 15px 30px; 
                            text-decoration: none; 
                            border-radius: 8px; 
                            display: inline-block; 
                            font-weight: bold; 
                            transition: opacity 0.3s; 
                        }}
                        .cta-button:hover {{ opacity: 0.9; }}
                        @media (max-width: 600px) {{
                            .price-info {{ flex-direction: column; }}
                            .container {{ margin: 0 10px; }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>{statusText}</h1>
                            <h2>{stockSymbol}</h2>
                        </div>
                        <div class='content'>
                            <div class='alert-info'>
                                <h3>Biến động Giá</h3>
                                <p>Cổ phiếu {stockSymbol} hiện đang giao dịch {changeText} giá mục tiêu của bạn.</p>
                                <div class='price-info'>
                                    <div class='price-item'>
                                        <strong>Giá Mục tiêu:</strong>
                                        <span style='font-size: 20px; font-weight: bold;'>${triggerPrice:F2}</span>
                                    </div>
                                    <div class='price-item'>
                                        <strong>Giá Hiện tại:</strong>
                                        <span style='font-size: 20px; font-weight: bold;'>${currentPrice:F2}</span>
                                    </div>
                                    <div class='price-item'>
                                        <strong>Chênh lệch:</strong>
                                        <span style='font-size: 20px; font-weight: bold;'>${Math.Abs(currentPrice - triggerPrice):F2}</span>
                                    </div>
                                </div>
                            </div>
                            <div style='text-align: center;'>
                                <a href='http://localhost:3000/portfolio' class='cta-button'>Xem Danh mục</a>
                            </div>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeEmailTemplate(string email)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ 
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                            line-height: 1.6; 
                            color: #333; 
                            margin: 0; 
                            padding: 0; 
                            background-color: #f4f4f4; 
                        }}
                        .container {{ 
                            max-width: 600px; 
                            margin: 0 auto; 
                            background-color: white; 
                            box-shadow: 0 0 20px rgba(0,0,0,0.1); 
                        }}
                        .header {{ 
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                            color: white; 
                            padding: 40px 30px; 
                            text-align: center; 
                            border-radius: 10px 10px 0 0; 
                        }}
                        .header h1 {{ margin: 0; font-size: 32px; }}
                        .content {{ 
                            background: #f8f9fa; 
                            padding: 40px 30px; 
                            border-radius: 0 0 10px 10px; 
                        }}
                        .content h2 {{ color: #667eea; margin-top: 0; }}
                        .features {{ 
                            background: white; 
                            padding: 25px; 
                            border-radius: 8px; 
                            margin: 25px 0; 
                            box-shadow: 0 2px 8px rgba(0,0,0,0.1); 
                        }}
                        .features ul {{ 
                            list-style: none; 
                            padding: 0; 
                            margin: 0; 
                        }}
                        .features li {{ 
                            padding: 8px 0; 
                            border-bottom: 1px solid #f0f0f0; 
                        }}
                        .features li:last-child {{ border-bottom: none; }}
                        .cta-button {{ 
                            background: #667eea; 
                            color: white; 
                            padding: 15px 40px; 
                            text-decoration: none; 
                            border-radius: 8px; 
                            display: inline-block; 
                            font-weight: bold; 
                            font-size: 16px; 
                            transition: background-color 0.3s; 
                        }}
                        .cta-button:hover {{ background: #5a6fd8; }}
                        .footer {{ 
                            text-align: center; 
                            margin-top: 30px; 
                            font-size: 12px; 
                            color: #666; 
                            padding: 20px; 
                            border-top: 1px solid #eee; 
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Chào mừng đến với Portfolio Tracker!</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {email}!</h2>
                            <p>Cảm ơn bạn đã tham gia Portfolio Tracker. Bây giờ bạn đã sẵn sàng để:</p>
                            <div class='features'>
                                <ul>
                                    <li>📊 Theo dõi các khoản đầu tư cổ phiếu của bạn</li>
                                    <li>📈 Giám sát hiệu suất danh mục đầu tư</li>
                                    <li>🔔 Thiết lập cảnh báo giá</li>
                                    <li>📝 Thêm ghi chú đầu tư</li>
                                    <li>📱 Truy cập dữ liệu thời gian thực</li>
                                </ul>
                            </div>
                            <p>Chúng tôi rất vui mừng được đồng hành cùng bạn trong hành trình đầu tư!</p>
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='http://localhost:3000/dashboard' class='cta-button'>Bắt đầu ngay</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>© 2025 Portfolio Tracker. Tất cả quyền được bảo lưu.</p>
                            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi!</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetTemplate(string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{ 
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                            line-height: 1.6; 
                            color: #333; 
                            margin: 0; 
                            padding: 0; 
                            background-color: #f4f4f4; 
                        }}
                        .container {{ 
                            max-width: 600px; 
                            margin: 0 auto; 
                            background-color: white; 
                            box-shadow: 0 0 20px rgba(0,0,0,0.1); 
                        }}
                        .header {{ 
                            background: #dc3545; 
                            color: white; 
                            padding: 30px; 
                            text-align: center; 
                            border-radius: 10px 10px 0 0; 
                        }}
                        .header h1 {{ margin: 0; font-size: 28px; }}
                        .content {{ 
                            background: #f8f9fa; 
                            padding: 30px; 
                            border-radius: 0 0 10px 10px; 
                        }}
                        .warning {{ 
                            background: #fff3cd; 
                            border: 2px solid #ffeaa7; 
                            padding: 20px; 
                            border-radius: 8px; 
                            margin: 20px 0; 
                        }}
                        .cta-button {{ 
                            background: #dc3545; 
                            color: white; 
                            padding: 15px 30px; 
                            text-decoration: none; 
                            border-radius: 8px; 
                            display: inline-block; 
                            font-weight: bold; 
                            font-size: 16px; 
                            transition: background-color 0.3s; 
                        }}
                        .cta-button:hover {{ background: #c82333; }}
                        .footer {{ 
                            text-align: center; 
                            margin-top: 30px; 
                            font-size: 12px; 
                            color: #666; 
                            padding: 20px; 
                            border-top: 1px solid #eee; 
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Yêu cầu Đặt lại Mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Portfolio Tracker của mình.</p>
                            <div class='warning'>
                                <strong>⚠️ Thông báo Bảo mật:</strong> Liên kết này sẽ hết hạn sau 1 giờ và chỉ có thể sử dụng một lần.
                            </div>
                            <p>Nhấp vào nút bên dưới để đặt lại mật khẩu của bạn:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' class='cta-button'>Đặt lại Mật khẩu</a>
                            </div>
                            <p><small><strong>Lưu ý:</strong> Nếu bạn không yêu cầu đặt lại mật khẩu này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</small></p>
                        </div>
                        <div class='footer'>
                            <p>© 2025 Portfolio Tracker. Tất cả quyền được bảo lưu.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}