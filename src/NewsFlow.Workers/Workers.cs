using NewsFlow.Core.Interfaces;
using NewsFlow.Infrastructure.ExternalServices;
using NewsFlow.Infrastructure.Pipeline;

namespace NewsFlow.Workers;

public class IngestWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<IngestWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public IngestWorker(IServiceProvider provider, ILogger<IngestWorker> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Ingest worker started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunIngestCycleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ingest cycle failed.");
            }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task RunIngestCycleAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var pipelineFactory = scope.ServiceProvider.GetRequiredService<IngestPipelineFactory>();

        var sources = await uow.Sources.GetAllActiveAsync(ct);

        foreach (var source in sources)
        {
            _logger.LogInformation("Ingesting from source: {SourceName}", source.Name);

            var stories = await FetchStoriesAsync(source.Url, ct);

            foreach (var story in stories)
            {
                var context = new IngestContext(
                    story.Title,
                    story.Content,
                    source.Name,
                    source.Url,
                    source.UserId);

                var pipeline = pipelineFactory.Build();
                await pipeline.HandleAsync(context, ct);

                if (context.ShouldStop)
                    _logger.LogDebug("Story stopped: {Reason}", context.StopReason);
            }

            source.MarkFetched();
            uow.Sources.Update(source);
        }

        await uow.CommitAsync(ct);
    }

    private Task<IEnumerable<(string Title, string Content)>> FetchStoriesAsync(string url, CancellationToken ct)
    {
        // TODO: Implement RSS / NewsAPI fetching via HttpClient
        return Task.FromResult(Enumerable.Empty<(string, string)>());
    }
}

public class SchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<SchedulerWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public SchedulerWorker(IServiceProvider provider, ILogger<SchedulerWorker> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Scheduler worker started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessDuePostsAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduler cycle failed.");
            }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessDuePostsAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
            var request = new PublishRequest(
                post.Content, post.VideoUrl, null, post.Hashtags, null);

            var result = await adapter.PublishAsync(account, request, ct);

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

public class VideoWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<VideoWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public VideoWorker(IServiceProvider provider, ILogger<VideoWorker> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Video worker started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessVideosAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video generation cycle failed.");
            }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessVideosAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var generatorFactory = scope.ServiceProvider.GetRequiredService<VideoGeneratorFactory>();

        var videoPosts = (await uow.Posts.GetScheduledBeforeAsync(DateTime.UtcNow.AddHours(1), ct))
            .Where(p => p.VideoUrl is null)
            .Take(3);

        foreach (var post in videoPosts)
        {
            var article = await uow.Articles.GetByIdAsync(post.ArticleId, ct);
            if (article is null) continue;

            var generator = generatorFactory.Create(post.Platform);
            var videoUrl = await generator.GenerateAsync(article, ct);

            post.AttachVideo(videoUrl);
            uow.Posts.Update(post);
            _logger.LogInformation("Video generated for post {PostId}", post.Id);
        }

        await uow.CommitAsync(ct);
    }
}
