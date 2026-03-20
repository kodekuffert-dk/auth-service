using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace auth_service.Services.Implementations;

public class MockEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MockEmailService> _logger;
    private readonly string _emailOutputPath;

    public MockEmailService(IConfiguration configuration, ILogger<MockEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _emailOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "mock-emails");
        
        // Ensure the directory exists
        if (!Directory.Exists(_emailOutputPath))
        {
            Directory.CreateDirectory(_emailOutputPath);
        }
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string confirmationToken)
    {
        var confirmationUrl = _configuration["Email:ConfirmationUrl"] ?? "http://localhost:5000/User/confirm-email";
        var confirmationLink = $"{confirmationUrl}?token={confirmationToken}&email={Uri.EscapeDataString(toEmail)}";

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var fileName = $"email-{timestamp}-{toEmail.Replace("@", "_at_")}.txt";
        var filePath = Path.Combine(_emailOutputPath, fileName);

        var emailContent = $@"========================================
MOCK EMAIL CONFIRMATION
========================================
To: {toEmail}
Subject: Confirm Your Email Address
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

========================================
EMAIL BODY:
========================================

Welcome!

Thank you for registering. Please confirm your email address by clicking the link below:

{confirmationLink}

Or use this token directly:
Token: {confirmationToken}
Email: {toEmail}

If you did not create an account, please ignore this email.

========================================
CLICK HERE TO CONFIRM (copy to browser):
========================================
{confirmationLink}

";

        await File.WriteAllTextAsync(filePath, emailContent);

        _logger.LogInformation("Mock email saved to file: {FilePath}", filePath);
        _logger.LogInformation("Confirmation link: {Link}", confirmationLink);

        // Log easy-to-find information
        Console.WriteLine("");
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          📧 MOCK EMAIL SENT - CHECK FILE                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"  To: {toEmail}");
        Console.WriteLine($"  File: mock-emails/{fileName}");
        Console.WriteLine($"  Link: {confirmationLink}");
        Console.WriteLine("═════════════════════════════════════════════════════════════════");
        Console.WriteLine("");
    }
}
