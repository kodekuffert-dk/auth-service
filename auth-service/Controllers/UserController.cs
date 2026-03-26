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
public class UserController(IUserRepository userRepository, ITeamRepository whitelistRepository, IAuthService authService, IEmailService emailService, ILogger<UserController> logger) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITeamRepository _whitelistRepository = whitelistRepository;
    private readonly IAuthService _authService = authService;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<UserController> _logger = logger;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto userDto)
    {
        _logger.LogInformation("Student creation requested for email: {Email}", userDto.Email);

        // check if email is on the whitelist
        var email = await _whitelistRepository.GetByEmail(userDto.Email);
        if (email == null)
        {
            _logger.LogWarning("Student creation failed - email not whitelisted: {Email}", userDto.Email);
            return BadRequest($"Email ({userDto.Email}) is not whitelisted");
        }

        // Students must be whitelisted - always set role to student
        User user = new()
        {
            Email = userDto.Email,
            Role = UserRole.Student,
            PasswordHash = _authService.HashPassword(userDto.Password),
            EmailConfirmationToken = _authService.GenerateToken(),
        };
        var userId = await _userRepository.CreateAsync(user);

        _logger.LogInformation("Student user created successfully with ID: {UserId}, Email: {Email}", userId, userDto.Email);

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);
            _logger.LogInformation("Email confirmation sent to: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to: {Email}", user.Email);
            return StatusCode(500, new { message = "User created but failed to send confirmation email. Please contact support." });
        }

        return Ok(new { message="User created successfully. Continue by confirming email."});
    }

    [HttpPost("privileged")]
    public async Task<IActionResult> CreatePrivilegedUser([FromBody] CreatePrivilegedUserDto userDto)
    {
        _logger.LogInformation("Privileged user creation requested for email: {Email} with role: {Role}", userDto.Email, userDto.Role);

        // Validate role
        if (string.IsNullOrWhiteSpace(userDto.Role))
        {
            _logger.LogWarning("Privileged user creation failed - role not provided for email: {Email}", userDto.Email);
            return BadRequest("Role is required for privileged users.");
        }

        if (!UserRole.IsValid(userDto.Role))
        {
            _logger.LogWarning("Privileged user creation failed - invalid role: {Role} for email: {Email}", userDto.Role, userDto.Email);
            return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRole.AllRoles)}");
        }

        var role = userDto.Role.ToLower();

        // Only allow creating teachers and admins through this endpoint
        if (role == UserRole.Student)
        {
            _logger.LogWarning("Privileged user creation failed - attempted to create student via privileged endpoint: {Email}", userDto.Email);
            return BadRequest("Students must be created through the regular user creation endpoint.");
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Privileged user creation failed - user already exists: {Email}", userDto.Email);
            return BadRequest($"User with email ({userDto.Email}) already exists.");
        }

        User user = new()
        {
            Email = userDto.Email,
            Role = role,
            PasswordHash = _authService.HashPassword(userDto.Password),
            EmailConfirmationToken = _authService.GenerateToken(),
        };
        var userId = await _userRepository.CreateAsync(user);

        _logger.LogInformation("Privileged user created successfully with ID: {UserId}, Email: {Email}, Role: {Role}", userId, userDto.Email, role);

        try
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, user.EmailConfirmationToken);
            _logger.LogInformation("Email confirmation sent to privileged user: {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to privileged user: {Email}", user.Email);
            return StatusCode(500, new { message = "User created but failed to send confirmation email. Please contact support." });
        }

        return Ok(new { message = $"{role} user created successfully. Continue by confirming email." });
    }

    [HttpPatch("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        _logger.LogInformation("Email confirmation requested for: {Email}", dto.Email);

        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.Email))
        {
            _logger.LogWarning("Email confirmation failed - missing token or email");
            return BadRequest(new { message = "Invalid confirmation request. Token and email are required." });
        }

        // Get user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("Email confirmation failed - user not found: {Email}", dto.Email);
            return NotFound(new { message = "User not found." });
        }

        // Check if email is already confirmed
        if (user.IsEmailConfirmed)
        {
            _logger.LogInformation("Email already confirmed for: {Email}", dto.Email);
            return Ok(new { message = "Email is already confirmed." });
        }

        // Validate the token
        if (user.EmailConfirmationToken != dto.Token)
        {
            _logger.LogWarning("Email confirmation failed - invalid token for: {Email}", dto.Email);
            return BadRequest(new { message = "Invalid or expired confirmation token." });
        }

        // Mark email as confirmed and clear the token
        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Email confirmed successfully for user: {Email} (ID: {UserId}, Role: {Role})", dto.Email, user.Id, user.Role);

        return Ok(new { message = $"Email ({dto.Email}) confirmed successfully." });
    }

    public class ConfirmEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
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

    public class CreatePrivilegedUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
