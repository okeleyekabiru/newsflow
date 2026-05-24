using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Interfaces;

public interface IAIProvider
{
    Task<string> GenerateTextAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
    Task<string> RewriteHeadlineAsync(string headline, CancellationToken ct = default);
    Task<string> GenerateCaptionAsync(string content, Platform platform, CancellationToken ct = default);
    Task<string> GenerateScriptAsync(Article article, CancellationToken ct = default);
}

public interface IVoiceProvider
{
    Task<byte[]> GenerateVoiceoverAsync(string script, string voiceId, CancellationToken ct = default);
}

public interface IStockFootageProvider
{
    Task<IEnumerable<string>> SearchVideosAsync(string query, int count = 5, CancellationToken ct = default);
    Task<IEnumerable<string>> SearchImagesAsync(string query, int count = 10, CancellationToken ct = default);
}

public interface IVideoAssembler
{
    Task<string> AssembleAsync(
        string scriptPath,
        string audioPath,
        IEnumerable<string> footageUrls,
        VideoFormat format,
        CancellationToken ct = default);
}

public enum VideoFormat { TikTok_9x16, YouTube_16x9, Shorts_9x16, Reels_9x16 }

public record PublishRequest(
    string Content,
    string? VideoUrl,
    string? ImageUrl,
    string[] Hashtags,
    DateTime? ScheduledAt);

public record PublishResult(
    bool IsSuccess,
    string? ExternalPostId,
    string? ErrorMessage);

public interface IPlatformAdapter
{
    Platform Platform { get; }
    Task<PublishResult> PublishAsync(Account account, PublishRequest request, CancellationToken ct = default);
    Task<bool> ValidateTokenAsync(Account account, CancellationToken ct = default);
    Task<long> GetFollowerCountAsync(Account account, CancellationToken ct = default);
}

public interface IPlatformAdapterFactory
{
    IPlatformAdapter Create(Platform platform);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

public interface IStorageService
{
    Task<string> UploadAsync(byte[] data, string fileName, string contentType, CancellationToken ct = default);
    Task<byte[]> DownloadAsync(string url, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}
