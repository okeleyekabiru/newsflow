using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;

namespace NewsFlow.API.Features.Articles;

public record CreateArticleCommand(
    Guid UserId,
    string Title,
    string ContentMd,
    ArticleCategory Category,
    ArticleTemplate Template) : IRequest<Result<Guid>>;

public record UpdateArticleCommand(
    Guid ArticleId,
    Guid UserId,
    string Title,
    string ContentMd,
    ArticleCategory Category) : IRequest<Result>;

public record PublishArticleCommand(
    Guid ArticleId,
    Guid UserId,
    Guid[] AccountIds,
    DateTime? ScheduledAt) : IRequest<Result>;

public record DeleteArticleCommand(
    Guid ArticleId,
    Guid UserId) : IRequest<Result>;

public record GetArticleQuery(Guid ArticleId, Guid UserId) : IRequest<Result<ArticleDto>>;
public record GetArticlesQuery(Guid UserId) : IRequest<Result<IEnumerable<ArticleDto>>>;
public record GetArticleVersionsQuery(Guid ArticleId, Guid UserId) : IRequest<Result<IEnumerable<ArticleVersionDto>>>;

public record ArticleDto(
    Guid Id,
    string Title,
    string ContentMd,
    string Status,
    string Category,
    string Template,
    int WordCount,
    DateTime UpdatedAt);

public record ArticleVersionDto(
    Guid Id,
    int WordCount,
    DateTime SavedAt);
