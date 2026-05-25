using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Interfaces;
using NewsFlow.Infrastructure.ExternalServices;
using NewsFlow.Infrastructure.Pipeline;

namespace NewsFlow.Workers;

// ── IngestWorker ─────────────────────────────────────────────────────────────

public class IngestWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<IngestWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public IngestWorker(IServiceProvider provider, ILogger<IngestWorker> logger)
    {
        _provider = provider;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Ingest worker started.");

        while (!ct.IsCancellationRequested)
        {
            try { await RunIngestCycleAsync(ct); }
            catch (Exception ex) { _logger.LogError(ex, "Ingest cycle failed."); }
            await Task.Delay(_interval, ct);
        }
    }

    /// <summary>Called by the Hangfire scheduler as well as the background loop.</summary>
    public async Task RunOnce(CancellationToken ct)
    {
        await RunIngestCycleAsync(ct);
    }

    private async Task RunIngestCycleAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow             = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var pipelineFactory = scope.ServiceProvider.GetRequiredService<IngestPipelineFactory>();
        var newsApiService  = scope.ServiceProvider.GetRequiredService<NewsApiService>();

        var sources = await uow.Sources.GetAllActiveAsync(ct);

        foreach (var source in sources)
        {
            _logger.LogInformation("Ingesting from source: {SourceName}", source.Name);

            IEnumerable<(string Title, string Content, string SourceName, string SourceUrl)> stories;

            try
            {
                stories = await FetchStoriesAsync(source, ct);
            }
            catch (Exception ex)
            {
                // One bad source must not stop others
                _logger.LogWarning(ex, "Failed to fetch from source {SourceName} — skipping.", source.Name);
                continue;
            }

            foreach (var story in stories)
            {
                var context = new IngestContext(
                    story.Title, story.Content, story.SourceName, story.SourceUrl, source.UserId);

                var pipeline = pipelineFactory.Build();
                await pipeline.HandleAsync(context, ct);

                if (context.ShouldStop)
                    _logger.LogDebug("Story stopped ({SourceName}): {Reason}", source.Name, context.StopReason);
            }

            source.MarkFetched();
            uow.Sources.Update(source);
        }

        // NewsAPI fallback: ingest top headlines not already covered by RSS sources
        try
        {
            var headlines = await newsApiService.FetchTopHeadlinesAsync(ct);
            foreach (var headline in headlines)
            {
                // Find which source user to attribute — use first active source owner for now
                var allSources = (await uow.Sources.GetAllActiveAsync(ct)).ToList();
                if (allSources.Count == 0) break;

                var ctx = new IngestContext(
                    headline.Title, headline.Content,
                    headline.SourceName, headline.SourceUrl,
                    allSources[0].UserId);

                var pipeline = pipelineFactory.Build();
                await pipeline.HandleAsync(ctx, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NewsAPI fallback failed — skipping.");
        }

        await uow.CommitAsync(ct);
    }

    /// <summary>
    /// Fetches stories from an RSS/Atom feed for a given <paramref name="source"/>.
    /// Items published in the last 2 hours only; max 10 per call.
    /// HTML content is stripped using HtmlAgilityPack.
    /// </summary>
    private async Task<IEnumerable<(string Title, string Content, string SourceName, string SourceUrl)>>
        FetchStoriesAsync(Source source, CancellationToken ct)
    {
        var feed = await FeedReader.ReadAsync(source.Url);
        var cutoff = DateTime.UtcNow.AddHours(-2);

        return feed.Items
            .Where(item => item.PublishingDate.HasValue && item.PublishingDate.Value.ToUniversalTime() >= cutoff)
            .Take(10)
            .Select(item =>
            {
                var raw = string.IsNullOrWhiteSpace(item.Content)
                    ? item.Description ?? string.Empty
                    : item.Content;
                var clean = StripHtml(raw);
                return (item.Title?.Trim() ?? "(no title)", clean, source.Name, source.Url);
            })
            .ToList();
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return Regex.Replace(doc.DocumentNode.InnerText, @"\s+", " ").Trim();
    }
}

// ── NewsApiService ────────────────────────────────────────────────────────────

/// <summary>
/// Fallback ingest from NewsAPI.org top-headlines endpoint.
/// Configure via appsettings: <c>NewsApi:ApiKey</c>.
/// </summary>
public class NewsApiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<NewsApiService> _logger;

    public NewsApiService(HttpClient http, IConfiguration config, ILogger<NewsApiService> logger)
    {
        _http    = http;
        _logger  = logger;
        _apiKey  = config["NewsApi:ApiKey"] ?? string.Empty;
        _http.BaseAddress = new Uri("https://newsapi.org");
    }

    /// <summary>Fetches up to 20 top headlines and maps them to the ingest tuple shape.</summary>
    public async Task<IEnumerable<(string Title, string Content, string SourceName, string SourceUrl)>>
        FetchTopHeadlinesAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogDebug("NewsApi:ApiKey not configured — skipping.");
            return [];
        }

        _logger.LogInformation("Fetching top headlines from NewsAPI.");

        var response = await _http.GetAsync(
            $"/v2/top-headlines?apiKey={_apiKey}&pageSize=20&language=en", ct);
        response.EnsureSuccessStatusCode();

        using var doc  = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var articles   = doc.RootElement.GetProperty("articles");

        var results = new List<(string, string, string, string)>();
        foreach (var a in articles.EnumerateArray())
        {
            var title      = a.TryGetProperty("title",       out var t) ? t.GetString() ?? "" : "";
            var content    = a.TryGetProperty("content",     out var c) ? c.GetString() ?? "" : "";
            var desc       = a.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";
            var sourceName = a.TryGetProperty("source", out var s) &&
                             s.TryGetProperty("name", out var sn) ? sn.GetString() ?? "NewsAPI" : "NewsAPI";
            var url        = a.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

            results.Add((title, string.IsNullOrWhiteSpace(content) ? desc : content, sourceName, url));
        }

        _logger.LogInformation("NewsAPI returned {Count} headlines.", results.Count);
        return results;
    }
}

// ── SchedulerWorker ───────────────────────────────────────────────────────────

public class SchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SchedulerWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public SchedulerWorker(IServiceProvider provider, ILogger<SchedulerWorker> logger)
    {
        _provider = provider;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Scheduler worker started.");

        while (!ct.IsCancellationRequested)
        {
            try { await ProcessDuePostsAsync(ct); }
            catch (Exception ex) { _logger.LogError(ex, "Scheduler cycle failed."); }
            await Task.Delay(_interval, ct);
        }
    }

    /// <summary>Public entry-point for the Hangfire recurring job.</summary>
    public async Task DispatchDuePostsAsync(CancellationToken ct) =>
        await ProcessDuePostsAsync(ct);

    private async Task ProcessDuePostsAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow     = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var factory = scope.ServiceProvider.GetRequiredService<IPlatformAdapterFactory>();

        var duePosts = await uow.Posts.GetScheduledBeforeAsync(DateTime.UtcNow, ct);

        foreach (var post in duePosts)
        {
            var account = await uow.Accounts.GetByIdAsync(post.AccountId, ct);
            if (account is null || !account.IsActive)
            {
                post.MarkFailed("Account not found or inactive.");
                uow.Posts.Update(post);
                continue;
            }

            var adapter = factory.Create(post.Platform);
            var request = new PublishRequest(post.Content, post.VideoUrl, null, post.Hashtags, null);
            var result  = await adapter.PublishAsync(account, request, ct);

            if (result.IsSuccess)
            {
                post.MarkPublished(result.ExternalPostId!);
                _logger.LogInformation("Published post {PostId} to {Platform}", post.Id, post.Platform);
            }
            else
            {
                post.MarkFailed(result.ErrorMessage ?? "Unknown error.");
                _logger.LogWarning("Failed post {PostId}: {Error}", post.Id, result.ErrorMessage);
            }

            uow.Posts.Update(post);
        }

        await uow.CommitAsync(ct);
    }
}

// ── VideoWorker ───────────────────────────────────────────────────────────────

public class VideoWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<VideoWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public VideoWorker(IServiceProvider provider, ILogger<VideoWorker> logger)
    {
        _provider = provider;
        _logger   = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Video worker started.");

        while (!ct.IsCancellationRequested)
        {
            try { await ProcessVideosAsync(ct); }
            catch (Exception ex) { _logger.LogError(ex, "Video generation cycle failed."); }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessVideosAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow              = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var generatorFactory = scope.ServiceProvider.GetRequiredService<VideoGeneratorFactory>();

        var videoPosts = (await uow.Posts.GetScheduledBeforeAsync(DateTime.UtcNow.AddHours(1), ct))
            .Where(p => p.VideoUrl is null)
            .Take(3);

        foreach (var post in videoPosts)
        {
            var article = await uow.Articles.GetByIdAsync(post.ArticleId, ct);
            if (article is null) continue;

            var generator = generatorFactory.Create(post.Platform);
            var videoUrl  = await generator.GenerateAsync(article, ct);

            post.AttachVideo(videoUrl);
            uow.Posts.Update(post);
            _logger.LogInformation("Video generated for post {PostId}", post.Id);
        }

        await uow.CommitAsync(ct);
    }
}
