using auth_service.Data;
using auth_service.Models;
using auth_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository userRepository, AuthService authService) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly AuthService _authService = authService;

    // POST: api/users/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            return BadRequest("Email og password skal udfyldes.");

        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
            return Unauthorized("Forkert email eller password.");

        if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
            return Unauthorized("Forkert email eller password.");

        if (!user.IsEmailConfirmed)
            return Unauthorized("E-mail er ikke bekræftet.");

        // Her kan evt. JWT-token genereres og returneres
        return Ok(new { message = "Login succesfuld", user.Id, user.Email, user.Role });
    }

    // POST: api/users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
            return BadRequest("Email og password skal udfyldes.");

        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return Conflict("Bruger med denne e-mail findes allerede.");

        var passwordHash = _authService.HashPassword(registerDto.Password);
        var confirmationToken = _authService.GenerateToken();
        var user = new User
        {
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = "Studerende",
            IsEmailConfirmed = false,
            EmailConfirmationToken = confirmationToken,
            CreatedAt = DateTime.UtcNow
        };
        var userId = await _userRepository.CreateAsync(user);
        // Her kan du evt. sende en e-mail med confirmationToken
        return Ok(new { message = "Bruger oprettet. Bekræft venligst din e-mail.", userId });
    }

    // GET: api/users/confirm-email?token=...
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token mangler.");

        var user = await _userRepository.GetByEmailConfirmationTokenAsync(token);
        if (user == null)
            return NotFound("Ugyldigt eller udløbet token.");

        if (user.IsEmailConfirmed)
            return BadRequest("E-mail er allerede bekræftet.");

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _userRepository.UpdateAsync(user);

        return Ok("E-mail er nu bekræftet. Du kan nu logge ind.");
    }
}
