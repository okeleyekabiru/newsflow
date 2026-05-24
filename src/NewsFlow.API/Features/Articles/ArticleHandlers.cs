using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;
using NewsFlow.API.Features.Articles;

namespace NewsFlow.API.Features.Articles;

public class CreateArticleHandler : IRequestHandler<CreateArticleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public CreateArticleHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> Handle(CreateArticleCommand cmd, CancellationToken ct)
    {
        var result = Article.Create(cmd.UserId, cmd.Title, cmd.ContentMd, cmd.Category, cmd.Template);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _uow.Articles.AddAsync(result.Value, ct);
        await _uow.CommitAsync(ct);
        return Result.Success(result.Value.Id);
    }
}

public class UpdateArticleHandler : IRequestHandler<UpdateArticleCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateArticleHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateArticleCommand cmd, CancellationToken ct)
    {
        var article = await _uow.Articles.GetByIdAsync(cmd.ArticleId, ct);
        if (article is null) return Result.Failure("Article not found.");
        if (article.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        var result = article.Update(cmd.Title, cmd.ContentMd, cmd.Category);
        if (result.IsFailure) return result;

        _uow.Articles.Update(article);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class PublishArticleHandler : IRequestHandler<PublishArticleCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly IContentFilterContext _filterContext;
    private readonly IPlatformAdapterFactory _adapterFactory;
    private readonly PostBuilder _postBuilder;

    public PublishArticleHandler(
        IUnitOfWork uow,
        IContentFilterContext filterContext,
        IPlatformAdapterFactory adapterFactory,
        PostBuilder postBuilder)
    {
        _uow = uow;
        _filterContext = filterContext;
        _adapterFactory = adapterFactory;
        _postBuilder = postBuilder;
    }

    public async Task<Result> Handle(PublishArticleCommand cmd, CancellationToken ct)
    {
        var article = await _uow.Articles.GetByIdAsync(cmd.ArticleId, ct);
        if (article is null) return Result.Failure("Article not found.");
        if (article.UserId != cmd.UserId) return Result.Failure("Unauthorized.");

        var filterResult = await _filterContext.ExecuteAsync(article, cmd.UserId, ct);

        if (filterResult.Decision == ContentDecision.Block)
            return Result.Failure($"Content blocked: {filterResult.Reason}");

        if (filterResult.Decision == ContentDecision.FlagForReview)
        {
            var flagResult = FlaggedPost.Create(
                article.Id,
                filterResult.Reason,
                filterResult.SeverityScore,
                article.Category,
                filterResult.TriggerKeywords,
                article.SourceName ?? "Unknown");

            if (flagResult.IsSuccess)
            {
                await _uow.FlaggedPosts.AddAsync(flagResult.Value, ct);
                await _uow.CommitAsync(ct);
            }
            return Result.Success();
        }

        foreach (var accountId in cmd.AccountIds)
        {
            var account = await _uow.Accounts.GetByIdAsync(accountId, ct);
            if (account is null || account.UserId != cmd.UserId) continue;

            var postResult = new PostBuilder()
                .ForArticle(article)
                .ForAccount(account)
                .WithSchedule(cmd.ScheduledAt ?? DateTime.UtcNow)
                .Build();

            if (postResult.IsFailure) continue;

            await _uow.Posts.AddAsync(postResult.Value, ct);
        }

        article.Publish();
        _uow.Articles.Update(article);
        await _uow.CommitAsync(ct);
        return Result.Success();
    }
}

public class GetArticleHandler : IRequestHandler<GetArticleQuery, Result<ArticleDto>>
{
    private readonly IUnitOfWork _uow;

    public GetArticleHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<ArticleDto>> Handle(GetArticleQuery query, CancellationToken ct)
    {
        var article = await _uow.Articles.GetByIdAsync(query.ArticleId, ct);
        if (article is null) return Result.Failure<ArticleDto>("Article not found.");
        if (article.UserId != query.UserId) return Result.Failure<ArticleDto>("Unauthorized.");

        return Result.Success(new ArticleDto(
            article.Id, article.Title, article.ContentMd,
            article.Status.ToString(), article.Category.ToString(),
            article.Template.ToString(), article.WordCount, article.UpdatedAt));
    }
}

public class GetArticlesHandler : IRequestHandler<GetArticlesQuery, Result<IEnumerable<ArticleDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetArticlesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IEnumerable<ArticleDto>>> Handle(GetArticlesQuery query, CancellationToken ct)
    {
        var articles = await _uow.Articles.GetByUserIdAsync(query.UserId, ct);
        var dtos = articles.Select(a => new ArticleDto(
            a.Id, a.Title, a.ContentMd, a.Status.ToString(),
            a.Category.ToString(), a.Template.ToString(), a.WordCount, a.UpdatedAt));

        return Result.Success(dtos);
    }
}
