using auth_service.Data.Models;

namespace auth_service.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<Guid> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<User?> GetByEmailConfirmationTokenAsync(string token);
}
