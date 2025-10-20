using auth_service.Data;
using auth_service.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace auth_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrator")]
    public class WhitelistController(ITeamRepository teamRepository) : ControllerBase
    {
        private readonly ITeamRepository _teamRepository = teamRepository;

        // POST: whitelist
        [HttpPost]
        public async Task<IActionResult> AddWhitelistEntries([FromBody] WhitelistEntriesDto dto)
        {
            if (dto.Emails == null || dto.Emails.Count == 0 || string.IsNullOrWhiteSpace(dto.TeamName))
                return BadRequest("Emails and TeamName are required.");

            int createdCount = await _teamRepository.AddEmailsAsync(dto.TeamName, dto.Emails.Select(e => e.Email));

            return Ok($"{createdCount} emails added to whitelist for team '{dto.TeamName}'");
        }

        // GET: whitelist
        [HttpGet]
        public async Task<ActionResult<List<WhitelistEntriesDto>>> GetWhitelist()
        {
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
                    whitelist.Emails.Add(new WhitelistEmailDto { Email = entry.Email, Status = entry.Status });
                }
                result.Add(whitelist);
            }
            return Ok(result);
        }

        // DELETE: whitelist
        [HttpDelete]
        public async Task<IActionResult> DeleteWhitelistEntries([FromBody] DeleteWhitelistEntriesDto dto)
        {
            int deletedCount = 0;
            foreach (var email in dto.Emails)
            {
                deletedCount += await _teamRepository.DeleteAsync(email);
            }

            return Ok($"{deletedCount} studienumre slettet fra whitelist");
        }

        public class WhitelistEntriesDto
        {
            public string TeamName { get; set; } = string.Empty;
            public List<WhitelistEmailDto> Emails { get; set; } = [];
        }

        public class WhitelistEmailDto
        {
            public string Email { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class DeleteWhitelistEntriesDto
        {
            public List<string> Emails { get; set; } = [];
        }
    }
}

