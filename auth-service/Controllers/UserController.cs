using auth_service.Data;
using auth_service.Data.Models;
using auth_service.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace auth_service.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize]
public class UserController(IUserRepository userRepository, ITeamRepository whitelistRepository, AuthService authService) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITeamRepository _whitelistRepository = whitelistRepository;
    private readonly AuthService _authService = authService;

    [HttpGet("me")]
    public async Task<IActionResult> GetUserAsync()
    {
        // retrieve current user information from a database or authentication service.
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID claim not found.");
        }
        var userEmail = userIdClaim.Value;
        
        // Fetch user details from the database using the email.
        if (userEmail == null) {
            return NotFound("User not found.");
        }
        var user = await _userRepository.GetByEmailAsync(userEmail);

        return Ok(new {
            user?.Id,
            user?.Email,
            user?.Role
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        // check if email is on the whitelist
        var email = await _whitelistRepository.GetByEmail(userDto.Email);
        if (email == null)
        {
            return BadRequest($"Email ({userDto.Email}) is not whitelisted");
        }
        // TODO: create a new user in the database.
        await _userRepository.CreateAsync(new User
        {
            Email = userDto.Email,
            Role = "Student", // default role
            PasswordHash = _authService.HashPassword(userDto.Password), // hash the password properly
            EmailConfirmationToken = _authService.GenerateToken(),
        });

        return Created();
    }

    [HttpPatch("me")]
    public IActionResult UpdateUser([FromBody] EditUserDto userDto)
    {
        // TODO: update user information in the database.
        throw new NotImplementedException();
    }

    public class EditUserDto
    {
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }
    }
}
