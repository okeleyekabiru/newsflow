using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Entities;

public class FlagAuditLog : BaseEntity
{
    public Guid FlaggedPostId { get; private set; }
    public string Action { get; private set; }
    public Guid ActorId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime Timestamp { get; private set; }

    private FlagAuditLog() { }

    public static FlagAuditLog Create(Guid flaggedPostId, string action, Guid actorId, string? notes)
    {
        return new FlagAuditLog
        {
            FlaggedPostId = flaggedPostId,
            Action = action,
            ActorId = actorId,
            Notes = notes,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class FlagRuleConfig : BaseEntity
{
    public Guid UserId { get; private set; }
    public ArticleCategory Category { get; private set; }
    public ContentDecision DefaultDecision { get; private set; } = ContentDecision.FlagForReview;
    public string[] TrustedSources { get; private set; } = [];
    public string[] BlockedKeywords { get; private set; } = [];
    public int SeverityThreshold { get; private set; } = 4;
    public string? EscalationEmail { get; private set; }
    public bool AutoPostTrustedSources { get; private set; } = false;

    private FlagRuleConfig() { }

    public static FlagRuleConfig CreateDefault(Guid userId, ArticleCategory category)
    {
        return new FlagRuleConfig
        {
            UserId = userId,
            Category = category,
            DefaultDecision = category is ArticleCategory.ConflictAndWar or ArticleCategory.Terrorism
                ? ContentDecision.FlagForReview
                : ContentDecision.AutoPost
        };
    }

    public void Update(
        ContentDecision defaultDecision,
        string[] trustedSources,
        string[] blockedKeywords,
        int severityThreshold,
        string? escalationEmail,
        bool autoPostTrustedSources)
    {
        DefaultDecision = defaultDecision;
        TrustedSources = trustedSources;
        BlockedKeywords = blockedKeywords;
        SeverityThreshold = severityThreshold;
        EscalationEmail = escalationEmail;
        AutoPostTrustedSources = autoPostTrustedSources;
        Touch();
    }
}

public class Source : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string Url { get; private set; }
    public string Type { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsTrusted { get; private set; } = false;
    public DateTime? LastFetchedAt { get; private set; }

    private Source() { }

    public static Result<Source> Create(Guid userId, string name, string url, string type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Source>("Source name is required.");
        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure<Source>("Source URL is required.");

        return Result.Success(new Source
        {
            UserId = userId,
            Name = name.Trim(),
            Url = url.Trim(),
            Type = type
        });
    }

    public void MarkTrusted() { IsTrusted = true; Touch(); }
    public void MarkFetched() { LastFetchedAt = DateTime.UtcNow; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
}

public class Analytics : BaseEntity
{
    public Guid PostId { get; private set; }
    public long Views { get; private set; }
    public long Likes { get; private set; }
    public long Shares { get; private set; }
    public long Comments { get; private set; }
    public decimal Revenue { get; private set; }
    public DateTime RecordedAt { get; private set; }

    public Post Post { get; private set; }

    private Analytics() { }

    public static Analytics Record(Guid postId, long views, long likes, long shares, long comments, decimal revenue)
    {
        return new Analytics
        {
            PostId = postId,
            Views = views,
            Likes = likes,
            Shares = shares,
            Comments = comments,
            Revenue = revenue,
            RecordedAt = DateTime.UtcNow
        };
    }
}
