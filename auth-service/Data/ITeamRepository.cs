using auth_service.Data.Models;

namespace auth_service.Data;

public interface ITeamRepository
{
    Task<int> AddEmailsAsync(string teamName, IEnumerable<string> emails);
    Task<int> DeleteAsync(string email);
    Task<IEnumerable<Team>> GetAllAsync();
    Task<WhitelistedEmail?> GetByEmail(string email);
}
