using System.Threading.Tasks;

namespace auth_service.Services;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string confirmationToken);
}
