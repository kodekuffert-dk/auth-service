using System.Data;
using auth_service.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;

namespace auth_service.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _db;
    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var sql = "SELECT * FROM users WHERE email = @email";
        var result = await _db.QueryFirstOrDefaultAsync<User>(sql, new { email });
        return result;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        var sql = "SELECT * FROM users WHERE id = @id";
        var result = await _db.QueryFirstOrDefaultAsync<User>(sql, new { id });
        return result;
    }

    public async Task<int> CreateAsync(User user)
    {
        var sql = @"INSERT INTO users (email, passwordhash, role, isemailconfirmed, emailconfirmationtoken, createdat)
                    VALUES (@Email, @PasswordHash, @Role, @IsEmailConfirmed, @EmailConfirmationToken, @CreatedAt)
                    RETURNING id;";
        var id = await _db.ExecuteScalarAsync<int>(sql, user);
        return id;
    }

    public async Task UpdateAsync(User user)
    {
        var sql = @"UPDATE users SET passwordhash = @PasswordHash, role = @Role, isemailconfirmed = @IsEmailConfirmed, emailconfirmationtoken = @EmailConfirmationToken WHERE id = @Id";
        await _db.ExecuteAsync(sql, user);
    }
}
