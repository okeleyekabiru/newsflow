using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ContentFilters;

public class ConflictFilterStrategy : IContentFilterStrategy
{
    public ArticleCategory Category => ArticleCategory.ConflictAndWar;

    private static readonly string[] ConflictKeywords =
    [
        "casualties", "airstrike", "bombing", "shelling", "offensive",
        "ceasefire", "troops", "militia", "warzone", "fatalities",
        "wounded", "siege", "evacuation", "refugee", "displaced",
        "frontline", "combat", "artillery", "missile", "drone strike"
    ];

    private static readonly string[] TrustedWarSources =
    [
        "Reuters", "AP News", "BBC News", "Al Jazeera", "AFP"
    ];

    public Task<FilterResult> EvaluateAsync(
        Article article,
        FlagRuleConfig? ruleConfig,
        CancellationToken ct = default)
    {
        var text = $"{article.Title} {article.ContentMd}".ToLowerInvariant();
        var triggeredKeywords = ConflictKeywords.Where(k => text.Contains(k)).ToArray();

        if (triggeredKeywords.Length == 0)
            return Task.FromResult(new FilterResult(
                ContentDecision.AutoPost, "No conflict keywords detected.", 0, []));

        var severityScore = Math.Min(10, triggeredKeywords.Length * 2);

        var trustedSources = ruleConfig?.TrustedSources ?? TrustedWarSources;
        var isTrustedSource = trustedSources.Any(s =>
            article.SourceName?.Contains(s, StringComparison.OrdinalIgnoreCase) == true);

        var threshold = ruleConfig?.SeverityThreshold ?? 4;

        if (ruleConfig?.AutoPostTrustedSources == true && isTrustedSource && severityScore < threshold)
            return Task.FromResult(new FilterResult(
                ContentDecision.AutoPost,
                "Trusted source — below severity threshold.",
                severityScore,
                triggeredKeywords));

        var blockedKeywords = ruleConfig?.BlockedKeywords ?? [];
        var hasBlockedContent = blockedKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        if (hasBlockedContent || severityScore >= 8)
            return Task.FromResult(new FilterResult(
                ContentDecision.Block,
                "Content contains blocked keywords or is extremely high severity.",
                severityScore,
                triggeredKeywords));

        return Task.FromResult(new FilterResult(
            ContentDecision.FlagForReview,
            $"Conflict content detected — {triggeredKeywords.Length} trigger keyword(s) found.",
            severityScore,
            triggeredKeywords));
    }
}

public class TerrorismFilterStrategy : IContentFilterStrategy
{
    public ArticleCategory Category => ArticleCategory.Terrorism;

    private static readonly string[] TerrorismKeywords =
    [
        "terrorist", "extremist", "jihad", "suicide bomber", "attack claimed",
        "propaganda", "radicalisation", "cell", "plot foiled", "manifesto"
    ];

    public Task<FilterResult> EvaluateAsync(
        Article article,
        FlagRuleConfig? ruleConfig,
        CancellationToken ct = default)
    {
        var text = $"{article.Title} {article.ContentMd}".ToLowerInvariant();
        var triggered = TerrorismKeywords.Where(k => text.Contains(k)).ToArray();

        if (triggered.Length == 0)
            return Task.FromResult(new FilterResult(ContentDecision.AutoPost, "Clean.", 0, []));

        var severity = Math.Min(10, triggered.Length * 3);

        if (severity >= 6)
            return Task.FromResult(new FilterResult(
                ContentDecision.Block,
                "High-severity terrorism content detected.",
                severity, triggered));

        return Task.FromResult(new FilterResult(
            ContentDecision.FlagForReview,
            "Potential terrorism content — requires editorial review.",
            severity, triggered));
    }
}

public class DefaultFilterStrategy : IContentFilterStrategy
{
    public ArticleCategory Category => ArticleCategory.General;

    public Task<FilterResult> EvaluateAsync(
        Article article,
        FlagRuleConfig? ruleConfig,
        CancellationToken ct = default)
    {
        return Task.FromResult(new FilterResult(
            ruleConfig?.DefaultDecision ?? ContentDecision.AutoPost,
            "Standard category — no special filtering applied.",
            0, []));
    }
}
