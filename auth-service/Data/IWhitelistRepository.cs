using auth_service.Models;

namespace auth_service.Data;

public interface IWhitelistRepository
{
    Task<WhitelistEntry?> GetByEmailAsync(string email);
    Task<int> AddAsync(WhitelistEntry entry);
    Task<int> DeleteAsync(string email);
    Task<IEnumerable<WhitelistEntry>> GetAllAsync();
}
