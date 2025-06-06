using System.Data;
using auth_service.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;

namespace auth_service.Data;

public interface IWhitelistRepository
{
    Task<WhitelistEntry?> GetByStudentNumberAsync(string studentNumber);
    Task AddAsync(WhitelistEntry entry);
    Task DeleteAsync(string studentNumber);
    Task<IEnumerable<WhitelistEntry>> GetAllAsync();
}

public class WhitelistRepository : IWhitelistRepository
{
    private readonly IDbConnection _db;
    public WhitelistRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<WhitelistEntry?> GetByStudentNumberAsync(string studentNumber)
    {
        var sql = "SELECT * FROM whitelist WHERE studentnumber = @studentNumber";
        return await _db.QueryFirstOrDefaultAsync<WhitelistEntry>(sql, new { studentNumber });
    }

    public async Task AddAsync(WhitelistEntry entry)
    {
        var sql = @"INSERT INTO whitelist (studentnumber, createdat) VALUES (@StudentNumber, @CreatedAt)";
        await _db.ExecuteAsync(sql, entry);
    }

    public async Task DeleteAsync(string studentNumber)
    {
        var sql = "DELETE FROM whitelist WHERE studentnumber = @studentNumber";
        await _db.ExecuteAsync(sql, new { studentNumber });
    }

    public async Task<IEnumerable<WhitelistEntry>> GetAllAsync()
    {
        var sql = "SELECT * FROM whitelist";
        return await _db.QueryAsync<WhitelistEntry>(sql);
    }
}
