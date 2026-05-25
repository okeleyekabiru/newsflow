using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Controllers;

/// <summary>
/// OAuth 2.0 callback endpoints for connecting social-media accounts.
/// Supports TikTok, Twitter (PKCE), Instagram, and YouTube.
/// PKCE code verifiers are stored in IDistributedCache (Redis) keyed by state.
/// </summary>
[ApiController]
[Route("api/oauth")]
[Authorize]
public class OAuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _uow;
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<OAuthController> _logger;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public OAuthController(
        IConfiguration config,
        IUnitOfWork uow,
        IDistributedCache cache,
        IHttpClientFactory httpFactory,
        ILogger<OAuthController> logger)
    {
        _config      = config;
        _uow         = uow;
        _cache       = cache;
        _httpFactory = httpFactory;
        _logger      = logger;
    }

    // ── TikTok ───────────────────────────────────────────────────────────────

    /// <summary>GET /api/oauth/tiktok/connect — redirect to TikTok authorization.</summary>
    [HttpGet("tiktok/connect")]
    public IActionResult TikTokConnect()
    {
        var clientKey   = _config["Platforms:TikTok:ClientKey"]!;
        var callbackUrl = BuildCallbackUrl("tiktok");
        var state       = UserId.ToString();

        var url = $"https://www.tiktok.com/v2/auth/authorize/" +
                  $"?client_key={clientKey}" +
                  $"&scope=user.info.basic,video.publish" +
                  $"&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                  $"&state={state}";

        return Redirect(url);
    }

    /// <summary>GET /api/oauth/tiktok/callback — exchange code for access token and save account.</summary>
    [HttpGet("tiktok/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> TikTokCallback(
        [FromQuery] string code, [FromQuery] string state, CancellationToken ct)
    {
        var userId = Guid.Parse(state);
        var http   = _httpFactory.CreateClient();

        var resp = await http.PostAsync(
            "https://open.tiktokapis.com/v2/oauth/token/",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_key"]    = _config["Platforms:TikTok:ClientKey"]!,
                ["client_secret"] = _config["Platforms:TikTok:ClientSecret"]!,
                ["code"]          = code,
                ["grant_type"]    = "authorization_code",
                ["redirect_uri"]  = BuildCallbackUrl("tiktok"),
            }), ct);

        if (!resp.IsSuccessStatusCode)
            return BadRequest("Failed to exchange TikTok authorization code.");

        using var doc       = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var accessToken     = doc.RootElement.GetProperty("access_token").GetString()!;
        var openId          = doc.RootElement.GetProperty("open_id").GetString() ?? "tiktok_user";

        await SaveAccountAsync(userId, Platform.TikTok, openId, accessToken, ct);
        return Redirect(FrontendUrl("/accounts?connected=tiktok"));
    }

    // ── Twitter (OAuth 2.0 PKCE) ──────────────────────────────────────────────

    /// <summary>GET /api/oauth/twitter/connect — redirect to X authorization with PKCE.</summary>
    [HttpGet("twitter/connect")]
    public async Task<IActionResult> TwitterConnect()
    {
        var (verifier, challenge) = GeneratePkce();
        var state = Guid.NewGuid().ToString("N");

        await _cache.SetStringAsync(
            $"pkce:{state}",
            verifier,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        var clientId    = _config["Platforms:Twitter:ApiKey"]!;
        var callbackUrl = BuildCallbackUrl("twitter");

        var url = "https://twitter.com/i/oauth2/authorize" +
                  $"?response_type=code" +
                  $"&client_id={Uri.EscapeDataString(clientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                  $"&scope=tweet.read%20tweet.write%20users.read%20offline.access" +
                  $"&state={state}" +
                  $"&code_challenge={challenge}" +
                  $"&code_challenge_method=S256";

        return Redirect(url);
    }

    /// <summary>GET /api/oauth/twitter/callback — exchange PKCE code for token.</summary>
    [HttpGet("twitter/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> TwitterCallback(
        [FromQuery] string code, [FromQuery] string state, CancellationToken ct)
    {
        var verifier = await _cache.GetStringAsync($"pkce:{state}");
        if (verifier is null) return BadRequest("Invalid or expired OAuth state.");
        await _cache.RemoveAsync($"pkce:{state}");

        var http        = _httpFactory.CreateClient();
        var callbackUrl = BuildCallbackUrl("twitter");
        var clientId    = _config["Platforms:Twitter:ApiKey"]!;
        var clientSecret= _config["Platforms:Twitter:ApiSecret"]!;

        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")));

        var resp = await http.PostAsync(
            "https://api.twitter.com/2/oauth2/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "authorization_code",
                ["code"]          = code,
                ["redirect_uri"]  = callbackUrl,
                ["code_verifier"] = verifier,
            }), ct);

        if (!resp.IsSuccessStatusCode)
            return BadRequest("Failed to exchange Twitter authorization code.");

        using var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

        // Use state-embedded userId (stored in cache as "user:{state}")
        var userIdStr = await _cache.GetStringAsync($"user:{state}") ?? UserId.ToString();
        var userId = Guid.Parse(userIdStr);

        await SaveAccountAsync(userId, Platform.Twitter, "@twitter_user", accessToken, ct);
        return Redirect(FrontendUrl("/accounts?connected=twitter"));
    }

    // ── Instagram ─────────────────────────────────────────────────────────────

    /// <summary>GET /api/oauth/instagram/connect — redirect to Instagram authorization.</summary>
    [HttpGet("instagram/connect")]
    public IActionResult InstagramConnect()
    {
        var appId       = _config["Platforms:Instagram:AppId"]!;
        var callbackUrl = BuildCallbackUrl("instagram");
        var state       = UserId.ToString();

        var url = "https://api.instagram.com/oauth/authorize" +
                  $"?client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                  $"&scope=user_profile,user_media" +
                  $"&response_type=code" +
                  $"&state={state}";

        return Redirect(url);
    }

    /// <summary>GET /api/oauth/instagram/callback — exchange code for long-lived token.</summary>
    [HttpGet("instagram/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> InstagramCallback(
        [FromQuery] string code, [FromQuery] string state, CancellationToken ct)
    {
        var userId = Guid.Parse(state);
        var http   = _httpFactory.CreateClient();

        var resp = await http.PostAsync(
            "https://api.instagram.com/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"]     = _config["Platforms:Instagram:AppId"]!,
                ["client_secret"] = _config["Platforms:Instagram:AppSecret"]!,
                ["grant_type"]    = "authorization_code",
                ["redirect_uri"]  = BuildCallbackUrl("instagram"),
                ["code"]          = code,
            }), ct);

        if (!resp.IsSuccessStatusCode)
            return BadRequest("Failed to exchange Instagram authorization code.");

        using var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var igUserId    = doc.RootElement.GetProperty("user_id").GetInt64().ToString();

        await SaveAccountAsync(userId, Platform.Instagram, igUserId, accessToken, ct);
        return Redirect(FrontendUrl("/accounts?connected=instagram"));
    }

    // ── YouTube / Google ──────────────────────────────────────────────────────

    /// <summary>GET /api/oauth/youtube/connect — redirect to Google OAuth.</summary>
    [HttpGet("youtube/connect")]
    public async Task<IActionResult> YouTubeConnect()
    {
        var (verifier, challenge) = GeneratePkce();
        var state = Guid.NewGuid().ToString("N");

        await _cache.SetStringAsync(
            $"pkce:{state}",
            verifier,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        await _cache.SetStringAsync(
            $"user:{state}",
            UserId.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

        var clientId    = _config["Platforms:YouTube:ClientId"]!;
        var callbackUrl = BuildCallbackUrl("youtube");

        var url = "https://accounts.google.com/o/oauth2/v2/auth" +
                  $"?client_id={Uri.EscapeDataString(clientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                  $"&response_type=code" +
                  $"&scope={Uri.EscapeDataString("https://www.googleapis.com/auth/youtube.upload https://www.googleapis.com/auth/youtube")}" +
                  $"&state={state}" +
                  $"&code_challenge={challenge}" +
                  $"&code_challenge_method=S256" +
                  $"&access_type=offline";

        return Redirect(url);
    }

    /// <summary>GET /api/oauth/youtube/callback — exchange code for YouTube access token.</summary>
    [HttpGet("youtube/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> YouTubeCallback(
        [FromQuery] string code, [FromQuery] string state, CancellationToken ct)
    {
        var verifier  = await _cache.GetStringAsync($"pkce:{state}");
        var userIdStr = await _cache.GetStringAsync($"user:{state}");
        if (verifier is null || userIdStr is null) return BadRequest("Invalid or expired OAuth state.");

        await _cache.RemoveAsync($"pkce:{state}");
        await _cache.RemoveAsync($"user:{state}");

        var http = _httpFactory.CreateClient();

        var resp = await http.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"]     = _config["Platforms:YouTube:ClientId"]!,
                ["client_secret"] = _config["Platforms:YouTube:ClientSecret"]!,
                ["grant_type"]    = "authorization_code",
                ["code"]          = code,
                ["redirect_uri"]  = BuildCallbackUrl("youtube"),
                ["code_verifier"] = verifier,
            }), ct);

        if (!resp.IsSuccessStatusCode)
            return BadRequest("Failed to exchange YouTube authorization code.");

        using var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var userId      = Guid.Parse(userIdStr);

        await SaveAccountAsync(userId, Platform.YouTube, "youtube_channel", accessToken, ct);
        return Redirect(FrontendUrl("/accounts?connected=youtube"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SaveAccountAsync(
        Guid userId, Platform platform, string handle, string accessToken, CancellationToken ct)
    {
        var existing = await _uow.Accounts.GetByPlatformAsync(userId, platform, ct);

        if (existing is not null)
        {
            existing.UpdateTokens(accessToken, null, null);
            _uow.Accounts.Update(existing);
        }
        else
        {
            var result = Account.Create(userId, platform, handle, accessToken);
            if (result.IsSuccess)
                await _uow.Accounts.AddAsync(result.Value, ct);
        }

        await _uow.CommitAsync(ct);
        _logger.LogInformation("Connected {Platform} account for user {UserId}", platform, userId);
    }

    private string BuildCallbackUrl(string platform) =>
        $"{_config["App:BaseUrl"] ?? "http://localhost:5000"}/api/oauth/{platform}/callback";

    private string FrontendUrl(string path) =>
        $"{_config["Cors:AllowedOrigins"]?.Split(',')[0] ?? "http://localhost:3000"}{path}";

    /// <summary>Generates a PKCE code_verifier and code_challenge (S256).</summary>
    private static (string Verifier, string Challenge) GeneratePkce()
    {
        var verifier  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var hash      = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = Convert.ToBase64String(hash)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (verifier, challenge);
    }
}
