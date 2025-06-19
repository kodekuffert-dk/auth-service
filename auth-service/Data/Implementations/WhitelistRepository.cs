using System.Data;
using auth_service.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;

namespace auth_service.Data.Implementations;

public class WhitelistRepository(IDbConnection db) : IWhitelistRepository
{
    private readonly IDbConnection _db = db;

    public async Task<WhitelistEntry?> GetByEmailAsync(string email)
    {
        var sql = "SELECT * FROM whitelist WHERE email = @email";
        return await _db.QueryFirstOrDefaultAsync<WhitelistEntry>(sql, new { email });
    }

    // AddAsync now uses entry.TeamName and entry.Emails
    public async Task AddAsync(WhitelistEntry entry)
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
        foreach (var email in entry.Emails)
        {
            await _db.ExecuteAsync(sql, new { Email = email, CreatedAt = entry.CreatedAt, TeamId = teamId });
        }
    }

    public async Task DeleteAsync(string email)
    {
        var sql = "DELETE FROM whitelist WHERE email = @email";
        await _db.ExecuteAsync(sql, new { email });
    }

    public async Task<IEnumerable<WhitelistEntry>> GetAllAsync()
    {
        var sql = "SELECT * FROM whitelist";
        return await _db.QueryAsync<WhitelistEntry>(sql);
    }
}
