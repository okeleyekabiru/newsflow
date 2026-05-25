using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using NewsFlow.Core.Interfaces;
using NewsFlow.Infrastructure.ContentFilters;
using NewsFlow.Infrastructure.Data;
using NewsFlow.Infrastructure.ExternalServices;
using NewsFlow.Infrastructure.Pipeline;
using NewsFlow.Infrastructure.Repositories;
using NewsFlow.Workers;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/workers-.log", rollingInterval: RollingInterval.Day))
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;
        var connString = config.GetConnectionString("PostgreSQL")!;

        services.AddDbContext<NewsFlowDbContext>(opts =>
            opts.UseNpgsql(connString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IContentFilterStrategy, ConflictFilterStrategy>();
        services.AddScoped<IContentFilterStrategy, TerrorismFilterStrategy>();
        services.AddScoped<IContentFilterStrategy, DefaultFilterStrategy>();
        services.AddScoped<IContentFilterContext, ContentFilterContext>();

        services.AddScoped<IPlatformAdapter, TikTokAdapter>();
        services.AddScoped<IPlatformAdapter, TwitterAdapter>();
        services.AddScoped<IPlatformAdapter, InstagramAdapter>();
        services.AddScoped<IPlatformAdapter, YouTubeAdapter>();
        services.AddScoped<IPlatformAdapterFactory, PlatformAdapterFactory>();

        services.AddHttpClient<IAIProvider, ClaudeAIProvider>();
        services.AddHttpClient<IVoiceProvider, ElevenLabsVoiceProvider>();
        services.AddHttpClient<IStockFootageProvider, PexelsFootageProvider>();

        services.AddSingleton<IVideoAssembler, FfmpegVideoAssembler>();
        services.AddScoped<IStorageService, R2StorageService>();

        services.AddScoped<IngestPipelineFactory>();
        services.AddScoped<VideoGeneratorFactory>();
        services.AddScoped<TikTokVideoGenerator>();
        services.AddScoped<YouTubeVideoGenerator>();
        services.AddScoped<ReelsVideoGenerator>();

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connString)));

        services.AddHangfireServer(opts =>
        {
            opts.WorkerCount = 5;
            opts.Queues = ["ingest", "video", "scheduler", "notifier", "default"];
        });

        services.AddHttpClient<NewsApiService>();

        services.AddHostedService<IngestWorker>();
        services.AddHostedService<SchedulerWorker>();
        services.AddHostedService<VideoWorker>();
    })
    .Build();

// ── Register recurring jobs ───────────────────────────────────────────────────
// Use IRecurringJobManager (DI-based) instead of the static RecurringJob API.
// The static API requires JobStorage.Current which is set only after host.Run()
// starts the Hangfire server — the DI-based manager resolves the storage from
// the registered IJobStorage directly, which is available right after Build().
using (var scope = host.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<IngestWorker>(
        recurringJobId: "ingest-cycle",
        methodCall:     x => x.RunOnce(CancellationToken.None),
        cronExpression: "*/5 * * * *",     // every 5 minutes
        timeZone:       TimeZoneInfo.Utc);

    recurringJobs.AddOrUpdate<SchedulerWorker>(
        recurringJobId: "scheduler-dispatch",
        methodCall:     x => x.DispatchDuePostsAsync(CancellationToken.None),
        cronExpression: "* * * * *",       // every minute
        timeZone:       TimeZoneInfo.Utc);
}

await host.RunAsync();
