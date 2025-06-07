using auth_service.Data;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;

    // POST: api/users/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] object loginDto)
    {
        // TODO: Implementer login-logik
        return Ok("Login endpoint");
    }

    // POST: api/users/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] object registerDto)
    {
        // TODO: Implementer register-logik
        return Ok("Register endpoint");
    }

    // GET: api/users/confirm-email?token=...
    [HttpGet("confirm-email")]
    public IActionResult ConfirmEmail([FromQuery] string token)
    {
        // TODO: Implementer e-mailbekræftelse
        return Ok("Email confirmation endpoint");
    }
}
