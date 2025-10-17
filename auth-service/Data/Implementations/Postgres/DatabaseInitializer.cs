using System.Data;
using Dapper;

namespace auth_service.Data.Implementations.Postgres;

public static class DatabaseInitializer
{
    public static void Initialize(IDbConnection db)
    {
        // Opret teams-tabel
        db.Execute(@"CREATE TABLE IF NOT EXISTS teams (
            id SERIAL PRIMARY KEY,
            name VARCHAR(100) NOT NULL UNIQUE,
            createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );");

        // Opret users-tabel med nullable team_id
        db.Execute(@"CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) NOT NULL UNIQUE,
            passwordhash VARCHAR(255) NOT NULL,
            role VARCHAR(50) NOT NULL,
            isemailconfirmed BOOLEAN NOT NULL DEFAULT FALSE,
            emailconfirmationtoken VARCHAR(255),
            createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            team_id INTEGER REFERENCES teams(id)
        );");

        // Opret whitelist-tabel med nullable team_id
        db.Execute(@"CREATE TABLE IF NOT EXISTS whitelist (
            id SERIAL PRIMARY KEY,
            email VARCHAR(255) NOT NULL UNIQUE,
            createdat TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            team_id INTEGER REFERENCES teams(id)
        );");

        // Seed admin-bruger hvis den ikke findes
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
                             PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123") // Default password, skal ændres ved første login
                         });
        }
    }
}
