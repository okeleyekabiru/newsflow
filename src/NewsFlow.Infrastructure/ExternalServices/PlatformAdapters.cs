using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace NewsFlow.Infrastructure.ExternalServices;

public class PlatformAdapterFactory : IPlatformAdapterFactory
{
    private readonly IEnumerable<IPlatformAdapter> _adapters;

    public PlatformAdapterFactory(IEnumerable<IPlatformAdapter> adapters) =>
        _adapters = adapters;

    public IPlatformAdapter Create(Platform platform) =>
        _adapters.FirstOrDefault(a => a.Platform == platform)
        ?? throw new NotSupportedException($"No adapter registered for platform: {platform}");
}

public class TikTokAdapter : IPlatformAdapter
{
    private readonly ILogger<TikTokAdapter> _logger;
    public Platform Platform => Platform.TikTok;

    public TikTokAdapter(ILogger<TikTokAdapter> logger) => _logger = logger;

    public async Task<PublishResult> PublishAsync(
        Account account,
        PublishRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Publishing to TikTok account @{Handle}", account.Handle);
            // TODO: TikTok Content Posting API v2
            // POST https://open.tiktokapis.com/v2/post/publish/video/init/
            await Task.Delay(100, ct);
            return new PublishResult(true, $"tiktok_{Guid.NewGuid()}", null);
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

public class TwitterAdapter : IPlatformAdapter
{
    private readonly ILogger<TwitterAdapter> _logger;
    public Platform Platform => Platform.Twitter;

    public TwitterAdapter(ILogger<TwitterAdapter> logger) => _logger = logger;

    public async Task<PublishResult> PublishAsync(
        Account account,
        PublishRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Publishing to X @{Handle}", account.Handle);
            // TODO: X API v2 POST /2/tweets
            await Task.Delay(100, ct);
            return new PublishResult(true, $"tweet_{Guid.NewGuid()}", null);
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

public class InstagramAdapter : IPlatformAdapter
{
    private readonly ILogger<InstagramAdapter> _logger;
    public Platform Platform => Platform.Instagram;

    public InstagramAdapter(ILogger<InstagramAdapter> logger) => _logger = logger;

    public async Task<PublishResult> PublishAsync(
        Account account,
        PublishRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Publishing to Instagram @{Handle}", account.Handle);
            // TODO: Instagram Graph API POST /{ig-user-id}/media
            await Task.Delay(100, ct);
            return new PublishResult(true, $"ig_{Guid.NewGuid()}", null);
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

public class YouTubeAdapter : IPlatformAdapter
{
    private readonly ILogger<YouTubeAdapter> _logger;
    public Platform Platform => Platform.YouTube;

    public YouTubeAdapter(ILogger<YouTubeAdapter> logger) => _logger = logger;

    public async Task<PublishResult> PublishAsync(
        Account account,
        PublishRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Publishing to YouTube @{Handle}", account.Handle);
            // TODO: YouTube Data API v3 POST /youtube/v3/videos
            await Task.Delay(100, ct);
            return new PublishResult(true, $"yt_{Guid.NewGuid()}", null);
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
