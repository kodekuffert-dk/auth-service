using auth_service.Data;
using auth_service.Models;
using auth_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace auth_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository userRepository, AuthService authService, IWhitelistRepository whitelistRepository) : ControllerBase
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly AuthService _authService = authService;
    private readonly IWhitelistRepository _whitelistRepository = whitelistRepository;

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
        return Ok(new { message = "Login succesfuld", token = tokenString, user.Id, user.Email, user.Role });
    }
}
