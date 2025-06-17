using auth_service.Models;

namespace auth_service.Data;

public interface IWhitelistRepository
{
    Task<WhitelistEntry?> GetByEmailAsync(string email);
    Task AddAsync(WhitelistEntry entry);
    Task DeleteAsync(string email);
    Task<IEnumerable<WhitelistEntry>> GetAllAsync();
}
