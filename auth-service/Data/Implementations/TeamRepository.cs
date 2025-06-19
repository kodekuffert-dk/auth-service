using System.Data;
using auth_service.Models;
using System.Threading.Tasks;
using Dapper;

namespace auth_service.Data.Implementations;

public class TeamRepository(IDbConnection db) : ITeamRepository
{
    private readonly IDbConnection _db = db;

    public async Task<int> CreateAsync(Team team)
    {
        var sql = @"INSERT INTO teams (name) VALUES (@Name) RETURNING id;";
        var id = await _db.ExecuteScalarAsync<int>(sql, team);
        return id;
    }
}
