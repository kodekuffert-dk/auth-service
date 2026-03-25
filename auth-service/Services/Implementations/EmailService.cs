using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace auth_service.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string confirmationToken)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPassword = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"] ?? "Auth Service";
        var confirmationUrl = _configuration["Email:ConfirmationUrl"];

        var confirmationLink = $"{confirmationUrl}?token={confirmationToken}&email={Uri.EscapeDataString(toEmail)}";

        var subject = "Confirm Your Email Address";
        var body = $@"
            <html>
            <body>
                <h2>Welcome!</h2>
                <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                <p><a href=""{confirmationLink}"">Confirm Email</a></p>
                <p>Or copy and paste this link into your browser:</p>
                <p>{confirmationLink}</p>
                <p>If you did not create an account, please ignore this email.</p>
            </body>
            </html>
        ";

        try
        {
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUser ?? "", fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Confirmation email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", toEmail);
            throw;
        }
    }
}
