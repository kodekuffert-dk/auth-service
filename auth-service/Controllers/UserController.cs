using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUser()
    {
        // TODO: retrieve current user information from a database or authentication service.
        var user = new
        {
            Id = 1,
            Email = ""
        };
        return Ok(user);
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
