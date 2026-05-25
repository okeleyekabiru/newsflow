using MediatR;
using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.API.Features.AI;

// ── Commands ─────────────────────────────────────────────────────────────────

public record RewriteHeadlineCommand(string Headline) : IRequest<Result<string>>;

public record GenerateCaptionCommand(string Content, Platform Platform) : IRequest<Result<string>>;

public record GenerateArticleCommand(string Topic, ArticleCategory Category) : IRequest<Result<string>>;

public record GenerateScriptCommand(Guid ArticleId) : IRequest<Result<string>>;

// ── Handlers ─────────────────────────────────────────────────────────────────

public class RewriteHeadlineHandler : IRequestHandler<RewriteHeadlineCommand, Result<string>>
{
    private readonly IAIProvider _ai;
    public RewriteHeadlineHandler(IAIProvider ai) => _ai = ai;

    public async Task<Result<string>> Handle(RewriteHeadlineCommand cmd, CancellationToken ct)
    {
        var result = await _ai.RewriteHeadlineAsync(cmd.Headline, ct);
        return Result.Success(result);
    }
}

public class GenerateCaptionHandler : IRequestHandler<GenerateCaptionCommand, Result<string>>
{
    private readonly IAIProvider _ai;
    public GenerateCaptionHandler(IAIProvider ai) => _ai = ai;

    public async Task<Result<string>> Handle(GenerateCaptionCommand cmd, CancellationToken ct)
    {
        var result = await _ai.GenerateCaptionAsync(cmd.Content, cmd.Platform, ct);
        return Result.Success(result);
    }
}

public class GenerateArticleHandler : IRequestHandler<GenerateArticleCommand, Result<string>>
{
    private readonly IAIProvider _ai;
    public GenerateArticleHandler(IAIProvider ai) => _ai = ai;

    public async Task<Result<string>> Handle(GenerateArticleCommand cmd, CancellationToken ct)
    {
        var result = await _ai.GenerateTextAsync(
            "You are a professional news writer. Write a factual, well-structured news article in Markdown. " +
            $"Category: {cmd.Category}. Keep it under 800 words.",
            $"Write a news article about: {cmd.Topic}", ct);
        return Result.Success(result);
    }
}

public class GenerateScriptHandler : IRequestHandler<GenerateScriptCommand, Result<string>>
{
    private readonly IAIProvider _ai;
    private readonly IUnitOfWork _uow;

    public GenerateScriptHandler(IAIProvider ai, IUnitOfWork uow)
    {
        _ai  = ai;
        _uow = uow;
    }

    public async Task<Result<string>> Handle(GenerateScriptCommand cmd, CancellationToken ct)
    {
        var article = await _uow.Articles.GetByIdAsync(cmd.ArticleId, ct);
        if (article is null) return Result.Failure<string>("Article not found.");

        var script = await _ai.GenerateScriptAsync(article, ct);
        return Result.Success(script);
    }
}
