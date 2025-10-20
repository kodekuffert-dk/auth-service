using auth_service.Data.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Linq;

namespace auth_service.Data.Implementations.Postgres;

public class TeamRepository(IDbConnection db) : ITeamRepository
{
    private readonly IDbConnection _db = db;

    // Adds emails to whitelist under a specific team. If the team does not exist, it is created.
    public async Task<int> AddEmailsAsync(string teamName, IEnumerable<string> emails)
    {
        // Try to get team id
        var teamId = await _db.ExecuteScalarAsync<Guid?>("SELECT id FROM teams WHERE name = @Name", new { Name = teamName });
        if (teamId == null)
        {
            // Create team if not exists
            teamId = await _db.ExecuteScalarAsync<Guid>("INSERT INTO teams (name) VALUES (@Name) RETURNING id;", new { Name = teamName });
        }

        // Insert one whitelist entry per email
        var sql = @"INSERT INTO whitelist (email, team_id) VALUES (@Email, @TeamId)";
        int createdCount = 0;
        foreach (var email in emails)
        {
            try
            {
                await _db.ExecuteAsync(sql, new { Email = email, TeamId = teamId });
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
    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        var sql = "SELECT teams.id AS TeamId, teams.name AS TeamName, whitelist.id, whitelist.email AS Email, whitelist.status " +
            "FROM whitelist " +
            "INNER JOIN teams ON teams.id = whitelist.team_id";

        var rows = await _db.QueryAsync<(Guid TeamId, string TeamName, Guid Id, string Email, string Status)>(sql);

        var entries = rows
            .GroupBy(r => (r.TeamId, r.TeamName))
            .Select(g => new Team
            {
                Id = g.Key.TeamId,
                Name = g.Key.TeamName,
                Emails = [.. g.Select(r => new WhitelistedEmail() { Id = r.Id, Email = r.Email, Status = r.Status })]
            })
            .Cast<Team>()
            .ToList();

        return entries;
    }

    public async Task<WhitelistedEmail?> GetByEmail(string email)
    {
        var sql = "SELECT id, email, status FROM whitelist WHERE email = @email";
        var entry = await _db.QuerySingleOrDefaultAsync<WhitelistedEmail>(sql, new { email });
        return entry;
    }
}
