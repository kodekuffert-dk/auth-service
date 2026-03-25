using System.Security.Cryptography;

namespace auth_service.Services;

public interface IAuthService
{
    public string HashPassword(string password);

    // Verificer et password mod et hash
    public bool VerifyPassword(string password, string hash);

    // Generer et simpelt token (til e-mailbekræftelse)
    public string GenerateToken(int size = 32);

    // Valider et token (kan udvides med udløb, signatur mv.)
    public bool ValidateToken(string token, string expectedToken);
}
