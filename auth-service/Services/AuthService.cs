using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace auth_service.Services;

public class AuthService
{
    // Hash et password med BCrypt
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verificer et password mod et hash
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    // Generer et simpelt token (til e-mailbekræftelse)
    public string GenerateToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(bytes);
    }

    // Valider et token (kan udvides med udløb, signatur mv.)
    public bool ValidateToken(string token, string expectedToken)
    {
        return token == expectedToken;
    }
}
