using auth_service.Models;

namespace auth_service.Data;

public interface ITeamRepository
{
    Task<int> CreateAsync(Team team);
}
