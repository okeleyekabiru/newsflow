using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Events;

public sealed record ArticlePublishedEvent(
    Guid ArticleId,
    Guid UserId,
    ArticleCategory Category) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record PostPublishedEvent(
    Guid PostId,
    Guid ArticleId,
    Guid AccountId,
    Platform Platform) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record PostFlaggedEvent(
    Guid FlaggedPostId,
    Guid ArticleId,
    ArticleCategory Category,
    int SeverityScore) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record FlagApprovedEvent(
    Guid FlaggedPostId,
    Guid ArticleId,
    Guid ReviewerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record FlagRejectedEvent(
    Guid FlaggedPostId,
    Guid ArticleId,
    Guid ReviewerId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record FlagEscalatedEvent(
    Guid FlaggedPostId,
    Guid ArticleId,
    Guid ReviewerId,
    string Notes) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
