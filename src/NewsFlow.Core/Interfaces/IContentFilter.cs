using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Interfaces;

public record FilterResult(
    ContentDecision Decision,
    string Reason,
    int SeverityScore,
    string[] TriggerKeywords);

public interface IContentFilterStrategy
{
    ArticleCategory Category { get; }
    Task<FilterResult> EvaluateAsync(Article article, FlagRuleConfig? ruleConfig, CancellationToken ct = default);
}

public interface IContentFilterContext
{
    Task<FilterResult> ExecuteAsync(Article article, Guid userId, CancellationToken ct = default);
}
