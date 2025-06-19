using auth_service.Data;
using auth_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace auth_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class WhitelistController : ControllerBase
    {
        private readonly IWhitelistRepository _whitelistRepository;

        public WhitelistController(IWhitelistRepository whitelistRepository)
        {
            _whitelistRepository = whitelistRepository;
        }

        // POST: api/whitelist
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
            await _whitelistRepository.AddAsync(entry);
            return Ok($"{dto.Emails.Count} emails added to whitelist for team '{dto.TeamName}'");
        }

        // GET: api/whitelist
        [HttpGet]
        public ActionResult<List<WhitelistEntryStatusDto>> GetWhitelist()
        {
            // TODO: Hent whitelist fra database og returner med status
            var dummyList = new List<WhitelistEntryStatusDto>
            {
                new WhitelistEntryStatusDto { Email = "123456", Status = "afventer" },
                new WhitelistEntryStatusDto { Email = "654321", Status = "aktiv" },
                new WhitelistEntryStatusDto { Email = "111111", Status = "inaktiv" }
            };
            return Ok(dummyList);
        }

        // DELETE: api/whitelist
        [HttpDelete]
        public IActionResult DeleteStudentNumbers([FromBody] DeleteStudentNumbersDto dto)
        {
            // TODO: Implementer logik til at slette studienumre fra whitelist
            return Ok($"{dto.Emails.Count} studienumre slettet fra whitelist");
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