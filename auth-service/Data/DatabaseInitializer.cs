using System.Data;
using Dapper;

namespace auth_service.Data;

public static class DatabaseInitializer
{
    public static void Initialize(IDbConnection db)
    {
        // Create the users table if it does not exist
        db.Execute(@"CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) NOT NULL UNIQUE,
            passwordhash VARCHAR(255) NOT NULL,
            role VARCHAR(50) NOT NULL,
            isemailconfirmed BOOLEAN NOT NULL DEFAULT FALSE,
            emailconfirmationtoken VARCHAR(255),
            createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );");

        db.Execute(@"CREATE TABLE IF NOT EXISTS whitelist (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) NOT NULL UNIQUE,
            createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );");

        // Seed the admin user if it does not exist
        var adminEmail = "lany@ucn.dk";
        var existingAdmin = db.QuerySingleOrDefault(
            "SELECT email FROM users WHERE email = @Email AND role = 'Administrator'",
            new { Email = adminEmail }
        );

        if (existingAdmin == null)
        {
            db.Execute(@"INSERT INTO users (email, passwordhash, role, isemailconfirmed)
                         VALUES (@Email, @PasswordHash, 'Administrator', TRUE)",
                         new
                         {
                             Email = adminEmail,
                             PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") // Use a secure hash for the password
                         });
        }
    }
}
