using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.Feed;

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetFeedQuery(
    Guid UserId,
    ArticleStatus? StatusFilter,
    ArticleCategory? CategoryFilter,
    int Page   = 1,
    int PerPage = 20) : IRequest<Result<FeedPageDto>>;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record FeedItemDto(
    Guid Id, string Title, string Status, string Category,
    string Template, int WordCount, string? SourceName, DateTime UpdatedAt);

public record FeedPageDto(IEnumerable<FeedItemDto> Items, int Total, int Page, int PerPage);

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetFeedHandler : IRequestHandler<GetFeedQuery, Result<FeedPageDto>>
{
    private readonly IUnitOfWork _uow;
    public GetFeedHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<FeedPageDto>> Handle(GetFeedQuery q, CancellationToken ct)
    {
        var all = (await _uow.Articles.GetByUserIdAsync(q.UserId, ct)).ToList();

        if (q.StatusFilter.HasValue)
            all = all.Where(a => a.Status == q.StatusFilter.Value).ToList();

        if (q.CategoryFilter.HasValue)
            all = all.Where(a => a.Category == q.CategoryFilter.Value).ToList();

        var total   = all.Count;
        var items   = all
            .Skip((q.Page - 1) * q.PerPage)
            .Take(q.PerPage)
            .Select(a => new FeedItemDto(
                a.Id, a.Title, a.Status.ToString(), a.Category.ToString(),
                a.Template.ToString(), a.WordCount, a.SourceName, a.UpdatedAt));

        return Result.Success(new FeedPageDto(items, total, q.Page, q.PerPage));
    }
}
