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
/// Connect flow: frontend calls POST /api/oauth/pre-auth (JWT-authenticated) to get a
/// short-lived token, then navigates the browser to GET /api/oauth/{platform}/connect?pre={token}.
/// This bridges JWT auth (header-based) with browser redirects that can't send headers.
/// </summary>
[ApiController]
[Route("api/oauth")]
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

    // ── Pre-auth bridge ───────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/oauth/pre-auth — called by the frontend (with JWT) to get a short-lived
    /// token that can be passed as a query param on the subsequent browser redirect to /connect.
    /// </summary>
    [HttpPost("pre-auth")]
    [Authorize]
    public async Task<IActionResult> PreAuth()
    {
        var token = Guid.NewGuid().ToString("N");
        await _cache.SetStringAsync(
            $"oauth-preauth:{token}",
            UserId.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        return Ok(new { token });
    }

    // ── TikTok ───────────────────────────────────────────────────────────────

    [HttpGet("tiktok/connect")]
    [AllowAnonymous]
    public async Task<IActionResult> TikTokConnect([FromQuery] string pre)
    {
        var userId = await ConsumePreAuthAsync(pre);
        if (userId is null) return BadRequest("Invalid or expired pre-auth token.");

        var state = Guid.NewGuid().ToString("N");
        await CacheUserStateAsync(state, userId.Value);

        var url = "https://www.tiktok.com/v2/auth/authorize/" +
                  $"?client_key={_config["Platforms:TikTok:ClientKey"]}" +
                  $"&scope=user.info.basic,video.publish" +
                  $"&response_type=code" +
                  $"&redirect_uri={Uri.EscapeDataString(BuildCallbackUrl("tiktok"))}" +
                  $"&state={state}";
        return Redirect(url);
    }

    [HttpGet("tiktok/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> TikTokCallback(
        [FromQuery] string? code, [FromQuery] string state, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
            return Redirect(FrontendUrl($"/accounts?error={Uri.EscapeDataString(error)}"));

        var userId = await ConsumeUserStateAsync(state);
        if (userId is null) return BadRequest("Invalid or expired OAuth state.");

        if (string.IsNullOrEmpty(code)) return BadRequest("Missing authorization code.");

        var http = _httpFactory.CreateClient();
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

        using var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var root        = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()!;
        var openId      = root.TryGetProperty("open_id",       out var oi) ? oi.GetString() ?? "tiktok_user" : "tiktok_user";
        var refreshTok  = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn   = root.TryGetProperty("expires_in",    out var ex) ? (int?)ex.GetInt32() : null;

        await SaveAccountAsync(userId.Value, Platform.TikTok, openId, accessToken,
            refreshTok, expiresIn.HasValue ? DateTime.UtcNow.AddSeconds(expiresIn.Value) : null, ct);
        return Redirect(FrontendUrl("/accounts?connected=tiktok"));
    }

    // ── Twitter (OAuth 2.0 PKCE) ──────────────────────────────────────────────

    [HttpGet("twitter/connect")]
    [AllowAnonymous]
    public async Task<IActionResult> TwitterConnect([FromQuery] string pre)
    {
        var userId = await ConsumePreAuthAsync(pre);
        if (userId is null) return BadRequest("Invalid or expired pre-auth token.");

        var (verifier, challenge) = GeneratePkce();
        var state = Guid.NewGuid().ToString("N");

        await _cache.SetStringAsync($"pkce:{state}", verifier,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        await CacheUserStateAsync(state, userId.Value);

        var url = "https://twitter.com/i/oauth2/authorize" +
                  $"?response_type=code" +
                  $"&client_id={Uri.EscapeDataString(_config["Platforms:Twitter:ApiKey"]!)}" +
                  $"&redirect_uri={Uri.EscapeDataString(BuildCallbackUrl("twitter"))}" +
                  $"&scope=tweet.read%20tweet.write%20users.read%20offline.access" +
                  $"&state={state}" +
                  $"&code_challenge={challenge}" +
                  $"&code_challenge_method=S256";
        return Redirect(url);
    }

    [HttpGet("twitter/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> TwitterCallback(
        [FromQuery] string? code, [FromQuery] string state, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
            return Redirect(FrontendUrl($"/accounts?error={Uri.EscapeDataString(error)}"));

        var verifier = await _cache.GetStringAsync($"pkce:{state}");
        var userId   = await ConsumeUserStateAsync(state);
        if (verifier is null || userId is null) return BadRequest("Invalid or expired OAuth state.");
        await _cache.RemoveAsync($"pkce:{state}");

        if (string.IsNullOrEmpty(code)) return BadRequest("Missing authorization code.");

        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{_config["Platforms:Twitter:ApiKey"]}:{_config["Platforms:Twitter:ApiSecret"]}")));

        var resp = await http.PostAsync(
            "https://api.twitter.com/2/oauth2/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "authorization_code",
                ["code"]          = code,
                ["redirect_uri"]  = BuildCallbackUrl("twitter"),
                ["code_verifier"] = verifier,
            }), ct);

        if (!resp.IsSuccessStatusCode)
            return BadRequest("Failed to exchange Twitter authorization code.");

        using var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var root        = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()!;
        var refreshTok  = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn   = root.TryGetProperty("expires_in",    out var ex) ? (int?)ex.GetInt32() : null;

        // Fetch the actual Twitter username
        var meClient = _httpFactory.CreateClient();
        meClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var meResp = await meClient.GetAsync("https://api.twitter.com/2/users/me", ct);
        var handle = "@twitter_user";
        if (meResp.IsSuccessStatusCode)
        {
            using var meDoc = JsonDocument.Parse(await meResp.Content.ReadAsStringAsync(ct));
            handle = "@" + (meDoc.RootElement
                .TryGetProperty("data", out var data) &&
                data.TryGetProperty("username", out var un)
                    ? un.GetString() ?? "twitter_user"
                    : "twitter_user");
        }

        await SaveAccountAsync(userId.Value, Platform.Twitter, handle, accessToken,
            refreshTok, expiresIn.HasValue ? DateTime.UtcNow.AddSeconds(expiresIn.Value) : null, ct);
        return Redirect(FrontendUrl("/accounts?connected=twitter"));
    }

    // ── Instagram ─────────────────────────────────────────────────────────────

    [HttpGet("instagram/connect")]
    [AllowAnonymous]
    public async Task<IActionResult> InstagramConnect([FromQuery] string pre)
    {
        var userId = await ConsumePreAuthAsync(pre);
        if (userId is null) return BadRequest("Invalid or expired pre-auth token.");

        var state = Guid.NewGuid().ToString("N");
        await CacheUserStateAsync(state, userId.Value);

        // Instagram API with Instagram Login (current as of 2025)
        var url = "https://api.instagram.com/oauth/authorize" +
                  $"?client_id={_config["Platforms:Instagram:AppId"]}" +
                  $"&redirect_uri={Uri.EscapeDataString(BuildCallbackUrl("instagram"))}" +
                  $"&scope=instagram_business_basic,instagram_business_content_publish" +
                  $"&response_type=code" +
                  $"&state={state}";
        return Redirect(url);
    }

    [HttpGet("instagram/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> InstagramCallback(
        [FromQuery] string? code, [FromQuery] string state, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
            return Redirect(FrontendUrl($"/accounts?error={Uri.EscapeDataString(error)}"));

        var userId = await ConsumeUserStateAsync(state);
        if (userId is null) return BadRequest("Invalid or expired OAuth state.");
        if (string.IsNullOrEmpty(code)) return BadRequest("Missing authorization code.");

        var http = _httpFactory.CreateClient();

        // Exchange code for short-lived token
        var tokenResp = await http.PostAsync(
            "https://api.instagram.com/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"]     = _config["Platforms:Instagram:AppId"]!,
                ["client_secret"] = _config["Platforms:Instagram:AppSecret"]!,
                ["grant_type"]    = "authorization_code",
                ["redirect_uri"]  = BuildCallbackUrl("instagram"),
                ["code"]          = code,
            }), ct);

        if (!tokenResp.IsSuccessStatusCode)
        {
            var errBody = await tokenResp.Content.ReadAsStringAsync(ct);
            _logger.LogError("Instagram token exchange failed: {Body}", errBody);
            return BadRequest("Failed to exchange Instagram authorization code.");
        }

        using var tokenDoc  = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync(ct));
        var shortToken      = tokenDoc.RootElement.GetProperty("access_token").GetString()!;
        var igUserId        = tokenDoc.RootElement.GetProperty("user_id").GetInt64().ToString();

        // Exchange for long-lived token (valid 60 days)
        var longResp = await http.GetAsync(
            $"https://graph.instagram.com/access_token" +
            $"?grant_type=ig_exchange_token" +
            $"&client_secret={_config["Platforms:Instagram:AppSecret"]}" +
            $"&access_token={shortToken}", ct);

        string accessToken;
        DateTime? expiry = null;
        if (longResp.IsSuccessStatusCode)
        {
            using var longDoc = JsonDocument.Parse(await longResp.Content.ReadAsStringAsync(ct));
            accessToken = longDoc.RootElement.GetProperty("access_token").GetString()!;
            if (longDoc.RootElement.TryGetProperty("expires_in", out var ex))
                expiry = DateTime.UtcNow.AddSeconds(ex.GetInt64());
        }
        else
        {
            accessToken = shortToken;
        }

        // Fetch username
        var meResp = await http.GetAsync(
            $"https://graph.instagram.com/v21.0/me?fields=username&access_token={accessToken}", ct);
        var handle = igUserId;
        if (meResp.IsSuccessStatusCode)
        {
            using var meDoc = JsonDocument.Parse(await meResp.Content.ReadAsStringAsync(ct));
            if (meDoc.RootElement.TryGetProperty("username", out var un))
                handle = "@" + (un.GetString() ?? igUserId);
        }

        await SaveAccountAsync(userId.Value, Platform.Instagram, handle, accessToken, null, expiry, ct);
        return Redirect(FrontendUrl("/accounts?connected=instagram"));
    }

    // ── YouTube / Google ──────────────────────────────────────────────────────

    [HttpGet("youtube/connect")]
    [AllowAnonymous]
    public async Task<IActionResult> YouTubeConnect([FromQuery] string pre)
    {
        var userId = await ConsumePreAuthAsync(pre);
        if (userId is null) return BadRequest("Invalid or expired pre-auth token.");

        var (verifier, challenge) = GeneratePkce();
        var state = Guid.NewGuid().ToString("N");

        await _cache.SetStringAsync($"pkce:{state}", verifier,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
        await CacheUserStateAsync(state, userId.Value);

        var url = "https://accounts.google.com/o/oauth2/v2/auth" +
                  $"?client_id={Uri.EscapeDataString(_config["Platforms:YouTube:ClientId"]!)}" +
                  $"&redirect_uri={Uri.EscapeDataString(BuildCallbackUrl("youtube"))}" +
                  $"&response_type=code" +
                  $"&scope={Uri.EscapeDataString("https://www.googleapis.com/auth/youtube.upload https://www.googleapis.com/auth/youtube")}" +
                  $"&state={state}" +
                  $"&code_challenge={challenge}" +
                  $"&code_challenge_method=S256" +
                  $"&access_type=offline" +
                  $"&prompt=consent";
        return Redirect(url);
    }

    [HttpGet("youtube/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> YouTubeCallback(
        [FromQuery] string? code, [FromQuery] string state, [FromQuery] string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
            return Redirect(FrontendUrl($"/accounts?error={Uri.EscapeDataString(error)}"));

        var verifier = await _cache.GetStringAsync($"pkce:{state}");
        var userId   = await ConsumeUserStateAsync(state);
        if (verifier is null || userId is null) return BadRequest("Invalid or expired OAuth state.");
        await _cache.RemoveAsync($"pkce:{state}");

        if (string.IsNullOrEmpty(code)) return BadRequest("Missing authorization code.");

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
        var root        = doc.RootElement;
        var accessToken = root.GetProperty("access_token").GetString()!;
        var refreshTok  = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn   = root.TryGetProperty("expires_in",    out var ex) ? (int?)ex.GetInt32() : null;

        // Fetch channel title
        var ytClient = _httpFactory.CreateClient();
        ytClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var chResp = await ytClient.GetAsync(
            "https://www.googleapis.com/youtube/v3/channels?part=snippet&mine=true", ct);
        var channelHandle = "youtube_channel";
        if (chResp.IsSuccessStatusCode)
        {
            using var chDoc = JsonDocument.Parse(await chResp.Content.ReadAsStringAsync(ct));
            if (chDoc.RootElement.TryGetProperty("items", out var items) &&
                items.GetArrayLength() > 0 &&
                items[0].TryGetProperty("snippet", out var snippet) &&
                snippet.TryGetProperty("title", out var title))
                channelHandle = title.GetString() ?? channelHandle;
        }

        await SaveAccountAsync(userId.Value, Platform.YouTube, channelHandle, accessToken,
            refreshTok, expiresIn.HasValue ? DateTime.UtcNow.AddSeconds(expiresIn.Value) : null, ct);
        return Redirect(FrontendUrl("/accounts?connected=youtube"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SaveAccountAsync(
        Guid userId, Platform platform, string handle,
        string accessToken, string? refreshToken, DateTime? expiry, CancellationToken ct)
    {
        var existing = await _uow.Accounts.GetByPlatformAsync(userId, platform, ct);
        if (existing is not null)
        {
            existing.UpdateTokens(accessToken, refreshToken, expiry);
            _uow.Accounts.Update(existing);
        }
        else
        {
            var result = Account.Create(userId, platform, handle, accessToken);
            if (result.IsSuccess)
            {
                result.Value.UpdateTokens(accessToken, refreshToken, expiry);
                await _uow.Accounts.AddAsync(result.Value, ct);
            }
        }
        await _uow.CommitAsync(ct);
        _logger.LogInformation("Connected {Platform} account for user {UserId}", platform, userId);
    }

    private async Task CacheUserStateAsync(string state, Guid userId) =>
        await _cache.SetStringAsync(
            $"user:{state}",
            userId.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

    private async Task<Guid?> ConsumeUserStateAsync(string state)
    {
        var cached = await _cache.GetStringAsync($"user:{state}");
        if (cached is null) return null;
        await _cache.RemoveAsync($"user:{state}");
        return Guid.TryParse(cached, out var id) ? id : null;
    }

    private async Task<Guid?> ConsumePreAuthAsync(string? pre)
    {
        if (string.IsNullOrWhiteSpace(pre)) return null;
        var cached = await _cache.GetStringAsync($"oauth-preauth:{pre}");
        if (cached is null) return null;
        await _cache.RemoveAsync($"oauth-preauth:{pre}");
        return Guid.TryParse(cached, out var id) ? id : null;
    }

    private string BuildCallbackUrl(string platform) =>
        $"{_config["App:BaseUrl"] ?? "http://localhost:5000"}/api/oauth/{platform}/callback";

    private string FrontendUrl(string path) =>
        $"{_config["Cors:AllowedOrigins"]?.Split(',')[0] ?? "http://localhost:3000"}{path}";

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
