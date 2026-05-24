using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
// 'User' inside a ControllerBase subclass resolves to the ClaimsPrincipal property.
// Alias our entity to avoid that ambiguity.
using AppUser = NewsFlow.Core.Entities.User;

namespace NewsFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<AppUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var userResult = AppUser.Create(req.Name, req.Email);
        if (userResult.IsFailure)
            return BadRequest(new { message = userResult.Error });

        // Identity hashes the password with PBKDF2 and persists the user atomically
        var identityResult = await _userManager.CreateAsync(userResult.Value, req.Password);
        if (!identityResult.Succeeded)
            return BadRequest(new { errors = identityResult.Errors.Select(e => e.Description) });

        var user = userResult.Value;
        var tokens = GenerateTokens(user);
        user.SetRefreshToken(tokens.RefreshToken, DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "30")));

        await _userManager.UpdateAsync(user);

        return Ok(tokens);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var tokens = GenerateTokens(user);
        user.SetRefreshToken(tokens.RefreshToken, DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "30")));

        await _userManager.UpdateAsync(user);

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var principal = GetPrincipalFromExpiredToken(req.AccessToken);
        if (principal is null)
            return Unauthorized(new { message = "Invalid access token." });

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null
            || user.RefreshToken != req.RefreshToken
            || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var tokens = GenerateTokens(user);
        user.SetRefreshToken(tokens.RefreshToken, DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "30")));

        await _userManager.UpdateAsync(user);

        return Ok(tokens);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return NotFound();

        user.ClearRefreshToken();
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Logged out successfully." });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private AuthTokens GenerateTokens(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("plan", user.Plan)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new AuthTokens(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            DateTime.UtcNow.AddMinutes(expiry));
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        try
        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }
}

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string AccessToken, string RefreshToken);
public record LogoutRequest(string Email);
public record AuthTokens(string AccessToken, string RefreshToken, DateTime ExpiresAt);
