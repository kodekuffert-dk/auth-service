using auth_service.Data;
using auth_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace auth_service.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController(IUserRepository userRepository, IAuthService authService, ILogger<LoginController> logger) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthService _authService = authService;
    private readonly ILogger<LoginController> _logger = logger;

    // POST: login
    [HttpPost()]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                _logger.LogWarning("Login failed - missing email or password");
                return BadRequest("Email og password skal udfyldes.");
            }

            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", loginDto.Email);
                return Unauthorized("Forkert email eller password.");
            }

            if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - invalid password for user: {Email}", loginDto.Email);
                return Unauthorized("Forkert email eller password.");
            }

            if (!user.IsEmailConfirmed)
            {
                _logger.LogWarning("Login failed - email not confirmed for user: {Email}", loginDto.Email);
                return Unauthorized("E-mail er ikke bekræftet.");
            }

            // JWT-token genereres og returneres
            var config = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            // Fallback key skal være mindst 32 tegn (256 bits)
            var jwtKey = (config != null && !string.IsNullOrWhiteSpace(config["Jwt:Key"])) ? config["Jwt:Key"] : "super_secret_dev_key_12345_67890!";
            var jwtIssuer = (config != null && !string.IsNullOrWhiteSpace(config["Jwt:Issuer"])) ? config["Jwt:Issuer"] : "auth_service";
            var jwtAudience = (config != null && !string.IsNullOrWhiteSpace(config["Jwt:Audience"])) ? config["Jwt:Audience"] : "auth_service_users";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Login successful for user: {Email} (ID: {UserId}, Role: {Role})", user.Email, user.Id, user.Role);

            return Ok(new { message = "Login succesfuld", token = tokenString, user.Id, user.Email, user.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", loginDto.Email);
            return StatusCode(500, new { message = "An error occurred during login. Please try again later." });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
