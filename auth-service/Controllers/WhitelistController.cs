using auth_service.Data;
using auth_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace auth_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrator")]
    public class WhitelistController : ControllerBase
    {
        private readonly IWhitelistRepository _whitelistRepository;

        public WhitelistController(IWhitelistRepository whitelistRepository)
        {
            _whitelistRepository = whitelistRepository;
        }

        // POST: whitelist
        [HttpPost]
        public async Task<IActionResult> AddWhitelistEntries([FromBody] AddWhitelistEntriesDto dto)
        {
            if (dto.Emails == null || dto.Emails.Count == 0 || string.IsNullOrWhiteSpace(dto.TeamName))
                return BadRequest("Emails and TeamName are required.");

            var entry = new WhitelistEntry
            {
                Emails = dto.Emails,
                TeamName = dto.TeamName
            };
            int createdCount = await _whitelistRepository.AddAsync(entry);
            return Ok($"{createdCount} emails added to whitelist for team '{dto.TeamName}'");
        }

        // GET: whitelist
        [HttpGet]
        public async Task<ActionResult<List<WhitelistEntryStatusDto>>> GetWhitelist()
        {
            var result = await _whitelistRepository.GetAllAsync();

            return Ok(result);
        }

        // DELETE: whitelist
        [HttpDelete]
        public async Task<IActionResult> DeleteStudentNumbers([FromBody] DeleteStudentNumbersDto dto)
        {
            int deletedCount = 0;
            foreach (var email in dto.Emails)
            {
                deletedCount += await _whitelistRepository.DeleteAsync(email);
            }
            
            return Ok($"{deletedCount} studienumre slettet fra whitelist");
        }
    }
}

public class AddWhitelistEntriesDto
{
    public List<string> Emails { get; set; } = new List<string>();
    public string TeamName { get; set; } = string.Empty;
}

public class DeleteStudentNumbersDto
{
    public List<string> Emails { get; set; } = new List<string>();
}

public class WhitelistEntryStatusDto
{
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // afventer, aktiv, inaktiv
}