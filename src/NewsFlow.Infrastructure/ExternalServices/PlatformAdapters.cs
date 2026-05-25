using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

// ── Factory ───────────────────────────────────────────────────────────────────

public class PlatformAdapterFactory : IPlatformAdapterFactory
{
    private readonly IEnumerable<IPlatformAdapter> _adapters;

    public PlatformAdapterFactory(IEnumerable<IPlatformAdapter> adapters) =>
        _adapters = adapters;

    public IPlatformAdapter Create(Platform platform) =>
        _adapters.FirstOrDefault(a => a.Platform == platform)
        ?? throw new NotSupportedException($"No adapter registered for platform: {platform}");
}

// ── TikTok ────────────────────────────────────────────────────────────────────

/// <summary>
/// TikTok Content Posting API v2.
/// Video: POST /v2/post/publish/video/init/  |  Text: POST /v2/post/publish/text/
/// </summary>
public class TikTokAdapter : IPlatformAdapter
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<TikTokAdapter> _logger;
    public Platform Platform => Platform.TikTok;

    public TikTokAdapter(IHttpClientFactory factory, ILogger<TikTokAdapter> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public async Task<PublishResult> PublishAsync(
        Account account, PublishRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Publishing to TikTok account @{Handle}", account.Handle);
        // Use the factory-managed client so Polly retry/CB policies are applied.
        var http = _factory.CreateClient("TikTok");
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account.AccessToken);

        try
        {
            if (!string.IsNullOrEmpty(request.VideoUrl))
            {
                var body = new
                {
                    post_info   = new { title = request.Content, privacy_level = "PUBLIC_TO_EVERYONE" },
                    source_info = new { source = "PULL_FROM_URL", video_url = request.VideoUrl }
                };
                var resp = await http.PostAsJsonAsync(
                    "https://open.tiktokapis.com/v2/post/publish/video/init/", body, ct);
                resp.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                var publishId = doc.RootElement.GetProperty("data").GetProperty("publish_id").GetString();
                return new PublishResult(true, publishId, null);
            }
            else
            {
                var body = new
                {
                    post_info = new { text = request.Content, privacy_level = "PUBLIC_TO_EVERYONE" }
                };
                var resp = await http.PostAsJsonAsync(
                    "https://open.tiktokapis.com/v2/post/publish/text/", body, ct);
                resp.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                var publishId = doc.RootElement.GetProperty("data").GetProperty("publish_id").GetString();
                return new PublishResult(true, publishId, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TikTok publish failed for @{Handle}", account.Handle);
            return new PublishResult(false, null, ex.Message);
        }
    }

    public Task<bool> ValidateTokenAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrEmpty(account.AccessToken));

    public Task<long> GetFollowerCountAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(account.FollowerCount);
}

// ── Twitter / X ───────────────────────────────────────────────────────────────

/// <summary>
/// X API v2 — POST /2/tweets.
/// Content is truncated to 280 characters by PostBuilder upstream.
/// </summary>
public class TwitterAdapter : IPlatformAdapter
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<TwitterAdapter> _logger;
    public Platform Platform => Platform.Twitter;

    public TwitterAdapter(IHttpClientFactory factory, ILogger<TwitterAdapter> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public async Task<PublishResult> PublishAsync(
        Account account, PublishRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Publishing to X @{Handle}", account.Handle);
        var http = _factory.CreateClient("Twitter");
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account.AccessToken);

        try
        {
            var resp = await http.PostAsJsonAsync(
                "https://api.twitter.com/2/tweets",
                new { text = request.Content }, ct);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            var tweetId = doc.RootElement.GetProperty("data").GetProperty("id").GetString();
            return new PublishResult(true, tweetId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twitter publish failed for @{Handle}", account.Handle);
            return new PublishResult(false, null, ex.Message);
        }
    }

    public Task<bool> ValidateTokenAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrEmpty(account.AccessToken));

    public Task<long> GetFollowerCountAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(account.FollowerCount);
}

// ── Instagram ─────────────────────────────────────────────────────────────────

/// <summary>
/// Instagram Graph API — two-step: create container then publish.
/// Requires instagram-account-id in account metadata (stored as Handle).
/// </summary>
public class InstagramAdapter : IPlatformAdapter
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<InstagramAdapter> _logger;
    public Platform Platform => Platform.Instagram;

    public InstagramAdapter(IHttpClientFactory factory, ILogger<InstagramAdapter> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public async Task<PublishResult> PublishAsync(
        Account account, PublishRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Publishing to Instagram @{Handle}", account.Handle);
        var http   = _factory.CreateClient("Instagram");
        var userId = account.Handle;   // handle stores the IG user-id (numeric string)
        var token  = account.AccessToken;
        var baseUrl = $"https://graph.instagram.com/v18.0/{userId}";

        try
        {
            // Step 1 — create media container
            var mediaPayload = new Dictionary<string, string>
            {
                ["caption"]      = request.Content,
                ["access_token"] = token
            };

            if (!string.IsNullOrEmpty(request.VideoUrl))
            {
                mediaPayload["video_url"]   = request.VideoUrl;
                mediaPayload["media_type"]  = "REELS";
            }
            else if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                mediaPayload["image_url"] = request.ImageUrl;
            }

            var step1 = await http.PostAsync($"{baseUrl}/media",
                new FormUrlEncodedContent(mediaPayload), ct);
            step1.EnsureSuccessStatusCode();

            using var doc1    = JsonDocument.Parse(await step1.Content.ReadAsStringAsync(ct));
            var creationId = doc1.RootElement.GetProperty("id").GetString();

            // Step 2 — publish the container
            var publishPayload = new Dictionary<string, string>
            {
                ["creation_id"]  = creationId!,
                ["access_token"] = token
            };
            var step2 = await http.PostAsync($"{baseUrl}/media_publish",
                new FormUrlEncodedContent(publishPayload), ct);
            step2.EnsureSuccessStatusCode();

            using var doc2 = JsonDocument.Parse(await step2.Content.ReadAsStringAsync(ct));
            var igPostId = doc2.RootElement.GetProperty("id").GetString();
            return new PublishResult(true, igPostId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Instagram publish failed for @{Handle}", account.Handle);
            return new PublishResult(false, null, ex.Message);
        }
    }

    public Task<bool> ValidateTokenAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrEmpty(account.AccessToken));

    public Task<long> GetFollowerCountAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(account.FollowerCount);
}

// ── YouTube ───────────────────────────────────────────────────────────────────

/// <summary>
/// YouTube Data API v3 — multipart upload (video-only; returns failure for text-only posts).
/// </summary>
public class YouTubeAdapter : IPlatformAdapter
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<YouTubeAdapter> _logger;
    public Platform Platform => Platform.YouTube;

    public YouTubeAdapter(IHttpClientFactory factory, ILogger<YouTubeAdapter> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    public async Task<PublishResult> PublishAsync(
        Account account, PublishRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Publishing to YouTube @{Handle}", account.Handle);

        if (string.IsNullOrEmpty(request.VideoUrl))
            return new PublishResult(false, null, "YouTube requires a video URL.");

        var http = _factory.CreateClient("YouTube");
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account.AccessToken);

        try
        {
            // Download video bytes
            var videoBytes = await http.GetByteArrayAsync(request.VideoUrl, ct);

            var metadata = JsonSerializer.Serialize(new
            {
                snippet = new
                {
                    title       = request.Content.Length > 100
                                    ? request.Content[..100]
                                    : request.Content,
                    description = request.Content,
                    categoryId  = "25"   // News & Politics
                },
                status = new { privacyStatus = "public" }
            });

            using var content = new MultipartContent("related");
            content.Add(new StringContent(metadata, System.Text.Encoding.UTF8, "application/json"));
            content.Add(new ByteArrayContent(videoBytes) { Headers = { ContentType = new MediaTypeHeaderValue("video/mp4") } });

            var resp = await http.PostAsync(
                "https://www.googleapis.com/upload/youtube/v3/videos?part=snippet,status&uploadType=multipart",
                content, ct);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
            var videoId = doc.RootElement.GetProperty("id").GetString();
            return new PublishResult(true, videoId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "YouTube publish failed for @{Handle}", account.Handle);
            return new PublishResult(false, null, ex.Message);
        }
    }

    public Task<bool> ValidateTokenAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrEmpty(account.AccessToken));

    public Task<long> GetFollowerCountAsync(Account account, CancellationToken ct = default) =>
        Task.FromResult(account.FollowerCount);
}
