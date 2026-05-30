using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.Pipeline;

public record IngestContext(
    string RawTitle,
    string RawContent,
    string SourceName,
    string SourceUrl,
    Guid UserId)
{
    public Article? Article { get; set; }
    public bool ShouldStop { get; set; }
    public string? StopReason { get; set; }
    public FilterResult? FilterResult { get; set; }
}

public abstract class IngestHandler
{
    private IngestHandler? _next;

    public IngestHandler SetNext(IngestHandler next)
    {
        _next = next;
        return next;
    }

    public async Task HandleAsync(IngestContext context, CancellationToken ct = default)
    {
        if (context.ShouldStop) return;
        await ProcessAsync(context, ct);
        if (!context.ShouldStop && _next is not null)
            await _next.HandleAsync(context, ct);
    }

    protected abstract Task ProcessAsync(IngestContext context, CancellationToken ct);
}

public class DuplicateCheckHandler : IngestHandler
{
    private readonly IUnitOfWork _uow;

    public DuplicateCheckHandler(IUnitOfWork uow) => _uow = uow;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        if (await _uow.Articles.ExistsByTitleAsync(context.RawTitle, ct))
        {
            context.ShouldStop = true;
            context.StopReason = "Duplicate article detected.";
        }
    }
}

public class SourceValidationHandler : IngestHandler
{
    private readonly IUnitOfWork _uow;

    public SourceValidationHandler(IUnitOfWork uow) => _uow = uow;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        var sources = await _uow.Sources.GetActiveByUserIdAsync(context.UserId, ct);
        var sourceExists = sources.Any(s =>
            s.Name.Equals(context.SourceName, StringComparison.OrdinalIgnoreCase));

        if (!sourceExists)
        {
            context.ShouldStop = true;
            context.StopReason = $"Source '{context.SourceName}' is not whitelisted.";
        }
    }
}

public class CategoryHandler : IngestHandler
{
    private static readonly Dictionary<string[], ArticleCategory> KeywordMap = new()
    {
        { ["war", "military", "troops", "combat", "ceasefire", "airstrike"], ArticleCategory.ConflictAndWar },
        { ["terrorist", "extremist", "attack", "bomb", "jihad"], ArticleCategory.Terrorism },
        { ["election", "parliament", "senate", "president", "government", "policy"], ArticleCategory.Politics },
        { ["stock", "nasdaq", "market", "economy", "inflation", "gdp", "trade"], ArticleCategory.Finance },
        { ["ai", "tech", "software", "apple", "google", "microsoft", "startup"], ArticleCategory.Technology },
        { ["goal", "match", "tournament", "league", "championship", "player"], ArticleCategory.Sports },
        { ["health", "vaccine", "hospital", "disease", "cancer", "mental health"], ArticleCategory.Health },
        { ["movie", "music", "celebrity", "oscar", "grammy", "film", "actor"], ArticleCategory.Entertainment },
    };

    protected override Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        var text = $"{context.RawTitle} {context.RawContent}".ToLowerInvariant();

        foreach (var (keywords, category) in KeywordMap)
        {
            if (keywords.Any(k => text.Contains(k)))
            {
                var articleResult = Article.Create(
                    context.UserId,
                    context.RawTitle,
                    context.RawContent,
                    category,
                    ArticleTemplate.BreakingNews);

                if (articleResult.IsSuccess)
                {
                    context.Article = articleResult.Value;
                    context.Article.SetSource(context.SourceName, context.SourceUrl);
                }
                return Task.CompletedTask;
            }
        }

        var defaultResult = Article.Create(
            context.UserId,
            context.RawTitle,
            context.RawContent,
            ArticleCategory.General,
            ArticleTemplate.BreakingNews);

        if (defaultResult.IsSuccess)
        {
            context.Article = defaultResult.Value;
            context.Article.SetSource(context.SourceName, context.SourceUrl);
        }

        return Task.CompletedTask;
    }
}

public class SafetyFilterHandler : IngestHandler
{
    private readonly IContentFilterContext _filterContext;

    public SafetyFilterHandler(IContentFilterContext filterContext) =>
        _filterContext = filterContext;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        if (context.Article is null) return;

        var result = await _filterContext.ExecuteAsync(context.Article, context.UserId, ct);
        context.FilterResult = result;

        if (result.Decision == ContentDecision.Block)
        {
            context.ShouldStop = true;
            context.StopReason = $"Blocked: {result.Reason}";
        }
    }
}

public class FlagCreationHandler : IngestHandler
{
    private readonly IUnitOfWork _uow;

    public FlagCreationHandler(IUnitOfWork uow) => _uow = uow;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        if (context.Article is null) return;
        if (context.FilterResult?.Decision != ContentDecision.FlagForReview) return;

        var result = FlaggedPost.Create(
            context.Article.Id,
            context.FilterResult.Reason,
            Math.Max(1, context.FilterResult.SeverityScore),
            context.Article.Category,
            context.FilterResult.TriggerKeywords,
            context.SourceName);

        if (result.IsSuccess)
            await _uow.FlaggedPosts.AddAsync(result.Value, ct);
    }
}

public class AIRewriteHandler : IngestHandler
{
    private readonly IAIProvider _ai;

    public AIRewriteHandler(IAIProvider ai) => _ai = ai;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        if (context.Article is null) return;

        try
        {
            var rewritten = await _ai.RewriteHeadlineAsync(context.Article.Title, ct);
            context.Article.Update(rewritten, context.Article.ContentMd, context.Article.Category);
        }
        catch
        {
            // AI rewrite is best-effort — keep original headline and continue to persistence
        }
    }
}

public class PersistenceHandler : IngestHandler
{
    private readonly IUnitOfWork _uow;

    public PersistenceHandler(IUnitOfWork uow) => _uow = uow;

    protected override async Task ProcessAsync(IngestContext context, CancellationToken ct)
    {
        if (context.Article is null) return;
        await _uow.Articles.AddAsync(context.Article, ct);
        await _uow.CommitAsync(ct);
    }
}

public class IngestPipelineFactory
{
    private readonly IUnitOfWork _uow;
    private readonly IContentFilterContext _filterContext;
    private readonly IAIProvider _ai;

    public IngestPipelineFactory(
        IUnitOfWork uow,
        IContentFilterContext filterContext,
        IAIProvider ai)
    {
        _uow = uow;
        _filterContext = filterContext;
        _ai = ai;
    }

    public IngestHandler Build()
    {
        var duplicate = new DuplicateCheckHandler(_uow);
        var source    = new SourceValidationHandler(_uow);
        var category  = new CategoryHandler();
        var safety    = new SafetyFilterHandler(_filterContext);
        var flag      = new FlagCreationHandler(_uow);
        var rewrite   = new AIRewriteHandler(_ai);
        var persist   = new PersistenceHandler(_uow);

        duplicate
            .SetNext(source)
            .SetNext(category)
            .SetNext(safety)
            .SetNext(flag)
            .SetNext(rewrite)
            .SetNext(persist);

        return duplicate;
    }
}
