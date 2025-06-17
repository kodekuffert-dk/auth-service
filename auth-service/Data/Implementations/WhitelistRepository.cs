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

    public async Task AddAsync(WhitelistEntry entry)
    {
        var sql = @"INSERT INTO whitelist (email, createdat) VALUES (@StudentNumber, @CreatedAt)";
        await _db.ExecuteAsync(sql, entry);
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
