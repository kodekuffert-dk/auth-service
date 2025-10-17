using auth_service.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Linq;

namespace auth_service.Data.Implementations.Postgres;

public class WhitelistRepository(IDbConnection db) : IWhitelistRepository
{
    private readonly IDbConnection _db = db;

    // Henter en whitelist entry baseret på email
    public async Task<WhitelistEntry?> GetByEmailAsync(string email)
    {
        var sql = "SELECT * FROM whitelist WHERE email = @email";
        return await _db.QueryFirstOrDefaultAsync<WhitelistEntry>(sql, new { email });
    }

    // AddAsync now uses entry.TeamName and entry.Emails
    public async Task<int> AddAsync(WhitelistEntry entry)
    {
        // Try to get team id
        var teamId = await _db.ExecuteScalarAsync<int?>("SELECT id FROM teams WHERE name = @Name", new { Name = entry.TeamName });
        if (teamId == null)
        {
            // Create team if not exists
            teamId = await _db.ExecuteScalarAsync<int>("INSERT INTO teams (name) VALUES (@Name) RETURNING id;", new { Name = entry.TeamName });
        }
        // Insert one whitelist entry per email
        var sql = @"INSERT INTO whitelist (email, createdat, team_id) VALUES (@Email, @CreatedAt, @TeamId)";
        int createdCount = 0;
        foreach (var email in entry.Emails)
        {
            try
            {
                await _db.ExecuteAsync(sql, new { Email = email, entry.CreatedAt, TeamId = teamId });
                createdCount++;
            }
            catch (PostgresException ex)
            {
                // Noget gik galt, fx duplicate key. Vi logger fejlen og fortsætter med næste email
                // TODO: Implement proper logging here
            }
        }
        return createdCount;
    }

    // Sletter en email fra whitelist
    public async Task<int> DeleteAsync(string email)
    {
        var sql = "DELETE FROM whitelist WHERE email = @email";
        return await _db.ExecuteAsync(sql, new { email });
    }

    // Henter alle whitelist entries grupperet efter team
    public async Task<IEnumerable<WhitelistEntry>> GetAllAsync()
    {
        var sql = "SELECT teams.id AS TeamId, teams.name AS TeamName, whitelist.email AS Email FROM whitelist INNER JOIN teams ON teams.id = whitelist.team_id";

        var rows = await _db.QueryAsync<(int TeamId, string TeamName, string Email)>(sql);

        var entries = rows
            .GroupBy(r => (r.TeamId, r.TeamName))
            .Select(g => new WhitelistEntry
            {
                Id = g.Key.TeamId,
                TeamName = g.Key.TeamName,
                Emails = [.. g.Select(x => x.Email)]
            })
            .Cast<WhitelistEntry>()
            .ToList();

        return entries;
    }
}
