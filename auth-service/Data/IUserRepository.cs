using auth_service.Models;

namespace auth_service.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<User?> GetByEmailConfirmationTokenAsync(string token);
}
