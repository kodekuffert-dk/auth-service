using auth_service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserRepository userRepository, IWhitelistRepository whitelistRepository) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IWhitelistRepository _whitelistRepository = whitelistRepository;

    [HttpGet("me")]
    public IActionResult GetUser()
    {
        // TODO: retrieve current user information from a database or authentication service.
        throw new NotImplementedException();
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult CreateUser([FromBody] dynamic userDto)
    {
        // TODO: create a new user in the database.
        throw new NotImplementedException();
    }

    [HttpPatch("{id}")]
    public IActionResult UpdateUser(int id, [FromBody] dynamic userDto)
    {
        // TODO: update user information in the database.
        throw new NotImplementedException();
    }
}
