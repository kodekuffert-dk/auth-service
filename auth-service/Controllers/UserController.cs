using auth_service.Data;
using auth_service.Data.Models;
using auth_service.Services;
using auth_service.Services.Implementations;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace auth_service.Controllers;
[Route("[controller]")]
[ApiController]
public class UserController(IUserRepository userRepository, ITeamRepository whitelistRepository, IAuthService authService, IEmailService emailService) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITeamRepository _whitelistRepository = whitelistRepository;
    private readonly IAuthService _authService = authService;
    private readonly IEmailService _emailService = emailService;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        // check if email is on the whitelist
        var email = await _whitelistRepository.GetByEmail(userDto.Email);
        if (email == null)
        {
            return BadRequest($"Email ({userDto.Email}) is not whitelisted");
        }

        User user = new()
        {
            Email = userDto.Email,
            Role = "Student", // default role
            PasswordHash = _authService.HashPassword(userDto.Password), // hash the password properly
            EmailConfirmationToken = _authService.GenerateToken(),
        };
        var userId = await _userRepository.CreateAsync(user);

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "User created but failed to send confirmation email. Please contact support." });
        }

        return Ok(new { message="User created successfully. Continue by confirming email."});
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
