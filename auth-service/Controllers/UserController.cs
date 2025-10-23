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

    [HttpPost]
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
        User user = new()
        {
            Email = userDto.Email,
            Role = "Student", // default role
            PasswordHash = _authService.HashPassword(userDto.Password), // hash the password properly
            EmailConfirmationToken = _authService.GenerateToken(),
        };
        var userId = await _userRepository.CreateAsync(user);

        // TODO: Something has to handle sending emails...

        return Created($"User/{userId}", user);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateUser([FromBody] EditUserDto userDto)
    {
        // retrieve current user information from database.
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID claim not found.");
        }
        var userEmail = userIdClaim.Value;

        // Fetch user details from the database using the email.
        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        user.PasswordHash = _authService.HashPassword(userDto.Password);

        await _userRepository.UpdateAsync(user);

        return Ok();
    }



    public class EditUserDto
    {
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
