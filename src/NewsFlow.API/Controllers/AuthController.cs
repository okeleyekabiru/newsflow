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

/// <summary>
/// Handles registration, login, token refresh, and logout.
/// Refresh tokens are stored in the Identity-managed AspNetUserTokens table
/// (provider = "NewsFlow", name = "RefreshToken") so no custom columns are needed.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string TokenProvider = "NewsFlow";
    private const string RefreshTokenName = "RefreshToken";

    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _logger = logger;
    }

    /// <summary>POST /api/auth/register — create account and return JWT + refresh token.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var userResult = AppUser.Create(req.Name, req.Email);
        if (userResult.IsFailure)
            return BadRequest(new { message = userResult.Error });

        // Identity hashes the password with PBKDF2 and persists the user atomically.
        var identityResult = await _userManager.CreateAsync(userResult.Value, req.Password);
        if (!identityResult.Succeeded)
            return BadRequest(new { errors = identityResult.Errors.Select(e => e.Description) });

        var user = userResult.Value;
        var tokens = await IssueTokensAsync(user);

        _logger.LogInformation("User registered: {Email}", user.Email);
        return Ok(tokens);
    }

    /// <summary>POST /api/auth/login — authenticate and return JWT + refresh token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        // CheckPasswordSignInAsync also handles lockout and 2FA checks.
        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, req.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for {Email}", req.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var tokens = await IssueTokensAsync(user);
        _logger.LogInformation("User logged in: {Email}", user.Email);
        return Ok(tokens);
    }

    /// <summary>POST /api/auth/refresh — validate refresh token and issue new pair.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var principal = GetPrincipalFromExpiredToken(req.AccessToken);
        if (principal is null)
            return Unauthorized(new { message = "Invalid access token." });

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized(new { message = "User not found." });

        // Retrieve the refresh token stored in AspNetUserTokens.
        var stored = await _userManager.GetAuthenticationTokenAsync(
            user, TokenProvider, RefreshTokenName);

        if (stored is null || stored != req.RefreshToken)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        var tokens = await IssueTokensAsync(user);
        _logger.LogInformation("Tokens refreshed for: {Email}", user.Email);
        return Ok(tokens);
    }

    /// <summary>POST /api/auth/logout — revoke the stored refresh token.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return NotFound();

        await _userManager.RemoveAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName);
        await _signInManager.SignOutAsync();

        _logger.LogInformation("User logged out: {Email}", req.Email);
        return Ok(new { message = "Logged out successfully." });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Generates a JWT + refresh token pair and stores the refresh token in Identity.</summary>
    private async Task<AuthTokens> IssueTokensAsync(AppUser user)
    {
        var accessToken = GenerateJwt(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        // Persist refresh token in AspNetUserTokens (replaces/upserts the existing entry).
        await _userManager.SetAuthenticationTokenAsync(
            user, TokenProvider, RefreshTokenName, refreshToken);

        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");
        return new AuthTokens(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(expiry));
    }

    private string GenerateJwt(AppUser user)
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

        return new JwtSecurityTokenHandler().WriteToken(token);
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
            ValidateLifetime = false   // allow expired tokens during refresh
        };

        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
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
