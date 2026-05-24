using NewsFlow.Core.Entities;
using NewsFlow.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace NewsFlow.Infrastructure.ExternalServices;

public abstract class VideoGeneratorBase
{
    protected readonly IAIProvider AI;
    protected readonly IVoiceProvider Voice;
    protected readonly IStockFootageProvider Footage;
    protected readonly IVideoAssembler Assembler;
    protected readonly IStorageService Storage;
    protected readonly ILogger Logger;

    protected VideoGeneratorBase(
        IAIProvider ai,
        IVoiceProvider voice,
        IStockFootageProvider footage,
        IVideoAssembler assembler,
        IStorageService storage,
        ILogger logger)
    {
        AI = ai;
        Voice = voice;
        Footage = footage;
        Assembler = assembler;
        Storage = storage;
        Logger = logger;
    }

    public async Task<string> GenerateAsync(Article article, CancellationToken ct = default)
    {
        Logger.LogInformation("Starting video generation for article {ArticleId}", article.Id);

        var script = await GenerateScriptAsync(article, ct);
        var audioData = await GenerateVoiceAsync(script, ct);
        var audioUrl = await Storage.UploadAsync(audioData, $"{article.Id}_audio.mp3", "audio/mpeg", ct);
        var footageUrls = await FetchFootageAsync(article.Title, ct);
        var videoUrl = await AssembleAsync(script, audioUrl, footageUrls, ct);
        var finalUrl = await ExportAsync(videoUrl, article.Id, ct);

        Logger.LogInformation("Video generated successfully for article {ArticleId}: {Url}", article.Id, finalUrl);
        return finalUrl;
    }

    protected virtual Task<string> GenerateScriptAsync(Article article, CancellationToken ct) =>
        AI.GenerateScriptAsync(article, ct);

    protected virtual async Task<byte[]> GenerateVoiceAsync(string script, CancellationToken ct) =>
        await Voice.GenerateVoiceoverAsync(script, GetVoiceId(), ct);

    protected virtual async Task<IEnumerable<string>> FetchFootageAsync(string query, CancellationToken ct) =>
        await Footage.SearchVideosAsync(query, GetFootageCount(), ct);

    protected virtual async Task<string> AssembleAsync(
        string script,
        string audioUrl,
        IEnumerable<string> footageUrls,
        CancellationToken ct) =>
        await Assembler.AssembleAsync(script, audioUrl, footageUrls, GetVideoFormat(), ct);

    protected virtual Task<string> ExportAsync(string videoPath, Guid articleId, CancellationToken ct) =>
        Storage.UploadAsync(
            System.IO.File.ReadAllBytes(videoPath),
            $"{articleId}_{GetFormatSuffix()}.mp4",
            "video/mp4",
            ct);

    protected abstract string GetVoiceId();
    protected abstract int GetFootageCount();
    protected abstract VideoFormat GetVideoFormat();
    protected abstract string GetFormatSuffix();
}

public class TikTokVideoGenerator : VideoGeneratorBase
{
    public TikTokVideoGenerator(
        IAIProvider ai, IVoiceProvider voice, IStockFootageProvider footage,
        IVideoAssembler assembler, IStorageService storage, ILogger<TikTokVideoGenerator> logger)
        : base(ai, voice, footage, assembler, storage, logger) { }

    protected override string GetVoiceId() => "rachel";
    protected override int GetFootageCount() => 4;
    protected override VideoFormat GetVideoFormat() => VideoFormat.TikTok_9x16;
    protected override string GetFormatSuffix() => "tiktok";
}

public class YouTubeVideoGenerator : VideoGeneratorBase
{
    public YouTubeVideoGenerator(
        IAIProvider ai, IVoiceProvider voice, IStockFootageProvider footage,
        IVideoAssembler assembler, IStorageService storage, ILogger<YouTubeVideoGenerator> logger)
        : base(ai, voice, footage, assembler, storage, logger) { }

    protected override string GetVoiceId() => "adam";
    protected override int GetFootageCount() => 8;
    protected override VideoFormat GetVideoFormat() => VideoFormat.YouTube_16x9;
    protected override string GetFormatSuffix() => "youtube";
}

public class ReelsVideoGenerator : VideoGeneratorBase
{
    public ReelsVideoGenerator(
        IAIProvider ai, IVoiceProvider voice, IStockFootageProvider footage,
        IVideoAssembler assembler, IStorageService storage, ILogger<ReelsVideoGenerator> logger)
        : base(ai, voice, footage, assembler, storage, logger) { }

    protected override string GetVoiceId() => "rachel";
    protected override int GetFootageCount() => 4;
    protected override VideoFormat GetVideoFormat() => VideoFormat.Reels_9x16;
    protected override string GetFormatSuffix() => "reels";
}

public class VideoGeneratorFactory
{
    private readonly IServiceProvider _provider;

    public VideoGeneratorFactory(IServiceProvider provider) => _provider = provider;

    public VideoGeneratorBase Create(Core.Enums.Platform platform) => platform switch
    {
        Core.Enums.Platform.TikTok => _provider.GetRequiredService<TikTokVideoGenerator>(),
        Core.Enums.Platform.YouTube => _provider.GetRequiredService<YouTubeVideoGenerator>(),
        Core.Enums.Platform.Instagram => _provider.GetRequiredService<ReelsVideoGenerator>(),
        _ => _provider.GetRequiredService<TikTokVideoGenerator>()
    };
}
