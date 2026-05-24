using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ContentFilters;

public class ContentFilterContext : IContentFilterContext
{
    private readonly IEnumerable<IContentFilterStrategy> _strategies;
    private readonly IUnitOfWork _uow;

    public ContentFilterContext(
        IEnumerable<IContentFilterStrategy> strategies,
        IUnitOfWork uow)
    {
        _strategies = strategies;
        _uow = uow;
    }

    public async Task<FilterResult> ExecuteAsync(
        Article article,
        Guid userId,
        CancellationToken ct = default)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Category == article.Category)
                    ?? _strategies.First(s => s.Category == ArticleCategory.General);

        var ruleConfig = await _uow.FlagRuleConfigs
            .GetByUserAndCategoryAsync(userId, article.Category, ct);

        return await strategy.EvaluateAsync(article, ruleConfig, ct);
    }
}
