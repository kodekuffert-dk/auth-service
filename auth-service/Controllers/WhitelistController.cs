using auth_service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhitelistController(IWhitelistRepository whitelistRepository) : ControllerBase
{
    private readonly IWhitelistRepository _whitelistRepository = whitelistRepository;

    // POST: api/whitelist
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult AddStudentNumbers([FromBody] AddStudentNumbersDto dto)
    {
        // TODO: Implementer logik til at tilføje studienumre til whitelist
        return Ok($"{dto.StudentNumbers.Count} studienumre tilføjet til whitelist");
    }

    // GET: api/whitelist
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public ActionResult<List<WhitelistEntryStatusDto>> GetWhitelist()
    {
        // TODO: Hent whitelist fra database og returner med status
        var dummyList = new List<WhitelistEntryStatusDto>
        {
            new WhitelistEntryStatusDto { StudentNumber = "123456", Status = "afventer" },
            new WhitelistEntryStatusDto { StudentNumber = "654321", Status = "aktiv" },
            new WhitelistEntryStatusDto { StudentNumber = "111111", Status = "inaktiv" }
        };
        return Ok(dummyList);
    }

    // DELETE: api/whitelist
    [HttpDelete]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteStudentNumbers([FromBody] DeleteStudentNumbersDto dto)
    {
        // TODO: Implementer logik til at slette studienumre fra whitelist
        return Ok($"{dto.StudentNumbers.Count} studienumre slettet fra whitelist");
    }
}

public class AddStudentNumbersDto
{
    public List<string> StudentNumbers { get; set; } = new List<string>();
}

public class DeleteStudentNumbersDto
{
    public List<string> StudentNumbers { get; set; } = new List<string>();
}

public class WhitelistEntryStatusDto
{
    public string StudentNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // afventer, aktiv, inaktiv
}