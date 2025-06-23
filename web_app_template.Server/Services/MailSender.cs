using Microsoft.AspNetCore.Identity.UI.Services;

namespace web_app_template.Server.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var sendGridKey = _configuration["SendGridKey"];
            ArgumentNullException.ThrowIfNullOrEmpty(sendGridKey, nameof(sendGridKey));
            await Execute(sendGridKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            await Task.Delay(1000);
            //send email logic
        }
    }
}
