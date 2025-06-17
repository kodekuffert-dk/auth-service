using auth_service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class WhitelistController(IWhitelistRepository whitelistRepository) : ControllerBase
{
    private readonly IWhitelistRepository _whitelistRepository = whitelistRepository;

    // POST: api/whitelist
    [HttpPost]
    public IActionResult AddStudentNumbers([FromBody] AddStudentNumbersDto dto)
    {
        // TODO: Implementer logik til at tilføje studienumre til whitelist
        return Ok($"{dto.Emails.Count} emails tilføjet til whitelist");
    }

    // GET: api/whitelist
    [HttpGet]
    public ActionResult<List<WhitelistEntryStatusDto>> GetWhitelist()
    {
        // TODO: Hent whitelist fra database og returner med status
        var dummyList = new List<WhitelistEntryStatusDto>
        {
            new() { Email = "123456", Status = "afventer" },
            new() { Email = "654321", Status = "aktiv" },
            new() { Email = "111111", Status = "inaktiv" }
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

public class AddStudentNumbersDto
{
    public List<string> Emails { get; set; } = new List<string>();
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