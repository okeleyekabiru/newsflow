using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ContentFilters;

public class ConflictFilterStrategy : IContentFilterStrategy
{
    public ArticleCategory Category => ArticleCategory.ConflictAndWar;

    private static readonly string[] ConflictKeywords =
    [
        // Categoriser keywords (broad)
        "war", "military", "troops", "combat", "ceasefire", "airstrike",
        // Safety-filter keywords (specific)
        "casualties", "bombing", "shelling", "offensive",
        "militia", "warzone", "fatalities", "wounded", "siege",
        "evacuation", "refugee", "displaced", "frontline",
        "artillery", "missile", "drone strike", "drones strike",
        "explosion", "attack", "assault", "invasion", "conflict"
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

        // Articles already categorised as ConflictAndWar always need review unless explicitly auto-posted
        var severityScore = triggeredKeywords.Length == 0
            ? 2
            : Math.Min(10, triggeredKeywords.Length * 2);

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
            triggeredKeywords.Length > 0
                ? $"Conflict content — {triggeredKeywords.Length} keyword(s) matched."
                : "Conflict/war category — requires editorial review.",
            severityScore,
            triggeredKeywords));
    }
}

public class TerrorismFilterStrategy : IContentFilterStrategy
{
    public ArticleCategory Category => ArticleCategory.Terrorism;

    private static readonly string[] TerrorismKeywords =
    [
        // Categoriser keywords (broad)
        "terrorist", "extremist", "attack", "bomb", "jihad",
        // Safety-filter keywords (specific)
        "suicide bomber", "attack claimed", "propaganda",
        "radicalisation", "cell", "plot foiled", "manifesto",
        "explosion", "shooting", "gunman", "hostage", "killed",
        "strike", "drone", "militant"
    ];

    public Task<FilterResult> EvaluateAsync(
        Article article,
        FlagRuleConfig? ruleConfig,
        CancellationToken ct = default)
    {
        var text = $"{article.Title} {article.ContentMd}".ToLowerInvariant();
        var triggered = TerrorismKeywords.Where(k => text.Contains(k)).ToArray();

        // Articles categorised as Terrorism always require review
        var severity = triggered.Length == 0
            ? 3
            : Math.Min(10, triggered.Length * 3);

        if (severity >= 6)
            return Task.FromResult(new FilterResult(
                ContentDecision.Block,
                "High-severity terrorism content detected.",
                severity, triggered));

        return Task.FromResult(new FilterResult(
            ContentDecision.FlagForReview,
            triggered.Length > 0
                ? $"Terrorism-related content — {triggered.Length} keyword(s) matched."
                : "Terrorism category — requires editorial review.",
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
