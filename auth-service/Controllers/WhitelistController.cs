using auth_service.Data;
using auth_service.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace auth_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WhitelistController(ITeamRepository teamRepository, ILogger<WhitelistController> logger) : ControllerBase
    {
        private readonly ITeamRepository _teamRepository = teamRepository;
        private readonly ILogger<WhitelistController> _logger = logger;

        // POST: whitelist
        [HttpPost]
        public async Task<IActionResult> AddWhitelistEntries([FromBody] CreateWhitelistEntriesDto dto)
        {
            _logger.LogInformation("Adding whitelist entries for team: {TeamName}, Email count: {EmailCount}", dto.TeamName, dto.Emails?.Count ?? 0);

            if (dto.Emails == null || dto.Emails.Count == 0 || string.IsNullOrWhiteSpace(dto.TeamName))
            {
                _logger.LogWarning("Whitelist addition failed - missing emails or team name");
                return BadRequest("Emails and TeamName are required.");
            }

            int createdCount = await _teamRepository.AddEmailsAsync(dto.TeamName, dto.Emails);

            _logger.LogInformation("Successfully added {CreatedCount} emails to whitelist for team: {TeamName}", createdCount, dto.TeamName);

            return Ok($"{createdCount} emails added to whitelist for team '{dto.TeamName}'");
        }

        // GET: whitelist
        [HttpGet]
        public async Task<ActionResult<List<WhitelistEntriesDto>>> GetWhitelist()
        {
            _logger.LogInformation("Fetching all whitelist entries");

            var teams = await _teamRepository.GetAllAsync();
            var result = new List<WhitelistEntriesDto>();
            foreach (var team in teams)
            {
                WhitelistEntriesDto whitelist = new()
                {
                    TeamName = team.Name
                };
                foreach (var entry in team.Emails)
                {
                    whitelist.Emails.Add(new WhitelistEmailDto(entry.Email, Enum.GetName(typeof(WhitelistStatus), entry.Status) ?? "Unknown"));
                }
                result.Add(whitelist);
            }

            _logger.LogInformation("Retrieved {TeamCount} teams with whitelist entries", result.Count);

            return Ok(result);
        }

        // DELETE: whitelist
        [HttpDelete]
        public async Task<IActionResult> DeleteWhitelistEntries([FromBody] DeleteWhitelistEntriesDto dto)
        {
            _logger.LogInformation("Deleting whitelist entries, Email count: {EmailCount}", dto.Emails?.Count ?? 0);

            int deletedCount = 0;
            foreach (var email in dto.Emails)
            {
                deletedCount += await _teamRepository.DeleteAsync(email);
            }

            _logger.LogInformation("Successfully deleted {DeletedCount} emails from whitelist", deletedCount);

            return Ok($"{deletedCount} emails deleted from whitelist");
        }

        public class WhitelistEntriesDto
        {
            public string TeamName { get; set; } = string.Empty;
            public List<WhitelistEmailDto> Emails { get; set; } = [];
        }

        public class WhitelistEmailDto(string email, string status)
        {
            public string Email { get; } = email;
            public string Status { get; } = status;
        }

        public class DeleteWhitelistEntriesDto
        {
            public List<string> Emails { get; set; } = [];
        }

        public enum WhitelistStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }

        public class CreateWhitelistEntriesDto
        {
            public string TeamName { get; set; } = string.Empty;
            public List<string> Emails { get; set; } = [];
        }
    }
}

