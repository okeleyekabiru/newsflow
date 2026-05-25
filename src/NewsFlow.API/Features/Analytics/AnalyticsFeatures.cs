using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Analytics;

// ── Queries ───────────────────────────────────────────────────────────────────

public record GetAnalyticsOverviewQuery(Guid UserId) : IRequest<Result<AnalyticsOverviewDto>>;

public record GetPostAnalyticsQuery(Guid UserId, Guid? PostId) : IRequest<Result<IEnumerable<PostAnalyticsDto>>>;

public record GetRevenueQuery(Guid UserId, DateTime From, DateTime To) : IRequest<Result<RevenueDto>>;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record AnalyticsOverviewDto(
    long TotalPosts, long TotalPublished, long TotalViews,
    long TotalLikes, long TotalShares, decimal TotalRevenue);

public record PostAnalyticsDto(
    Guid PostId, string Platform, long Views, long Likes,
    long Shares, long Comments, decimal Revenue, DateTime RecordedAt);

public record RevenueDto(decimal Total, DateTime From, DateTime To);

// ── Handlers ─────────────────────────────────────────────────────────────────

public class GetAnalyticsOverviewHandler
    : IRequestHandler<GetAnalyticsOverviewQuery, Result<AnalyticsOverviewDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAnalyticsOverviewHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AnalyticsOverviewDto>> Handle(
        GetAnalyticsOverviewQuery q, CancellationToken ct)
    {
        var posts       = await _uow.Posts.GetByAccountIdAsync(Guid.Empty, ct); // all posts
        var accountPosts = new List<NewsFlow.Core.Entities.Post>();

        var accounts = await _uow.Accounts.GetByUserIdAsync(q.UserId, ct);
        foreach (var acc in accounts)
        {
            var ap = await _uow.Posts.GetByAccountIdAsync(acc.Id, ct);
            accountPosts.AddRange(ap);
        }

        var analytics   = await _uow.Analytics.GetAllAsync(ct);
        var userAnalytics = analytics
            .Where(a => accountPosts.Any(p => p.Id == a.PostId))
            .ToList();

        var overview = new AnalyticsOverviewDto(
            TotalPosts:     accountPosts.Count,
            TotalPublished: accountPosts.Count(p => p.Status == NewsFlow.Core.Enums.PostStatus.Published),
            TotalViews:     userAnalytics.Sum(a => a.Views),
            TotalLikes:     userAnalytics.Sum(a => a.Likes),
            TotalShares:    userAnalytics.Sum(a => a.Shares),
            TotalRevenue:   userAnalytics.Sum(a => a.Revenue));

        return Result.Success(overview);
    }
}

public class GetPostAnalyticsHandler
    : IRequestHandler<GetPostAnalyticsQuery, Result<IEnumerable<PostAnalyticsDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetPostAnalyticsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<PostAnalyticsDto>>> Handle(
        GetPostAnalyticsQuery q, CancellationToken ct)
    {
        IEnumerable<NewsFlow.Core.Entities.Analytics> records;

        if (q.PostId.HasValue)
        {
            records = await _uow.Analytics.GetByPostIdAsync(q.PostId.Value, ct);
        }
        else
        {
            records = await _uow.Analytics.GetAllAsync(ct);
        }

        var accounts = await _uow.Accounts.GetByUserIdAsync(q.UserId, ct);
        var accountIds = accounts.Select(a => a.Id).ToHashSet();

        var dtos = new List<PostAnalyticsDto>();
        foreach (var r in records)
        {
            var post = await _uow.Posts.GetByIdAsync(r.PostId, ct);
            if (post is null || !accountIds.Contains(post.AccountId)) continue;

            dtos.Add(new PostAnalyticsDto(
                r.PostId, post.Platform.ToString(),
                r.Views, r.Likes, r.Shares, r.Comments, r.Revenue, r.RecordedAt));
        }

        return Result.Success<IEnumerable<PostAnalyticsDto>>(dtos);
    }
}

public class GetRevenueHandler : IRequestHandler<GetRevenueQuery, Result<RevenueDto>>
{
    private readonly IUnitOfWork _uow;
    public GetRevenueHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<RevenueDto>> Handle(GetRevenueQuery q, CancellationToken ct)
    {
        var total = await _uow.Analytics.GetTotalRevenueAsync(q.UserId, q.From, q.To, ct);
        return Result.Success(new RevenueDto(total, q.From, q.To));
    }
}
