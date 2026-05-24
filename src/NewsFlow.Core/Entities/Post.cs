using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Events;

namespace NewsFlow.Core.Entities;

public class Post : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public Guid AccountId { get; private set; }
    public Platform Platform { get; private set; }
    public string Content { get; private set; }
    public string? VideoUrl { get; private set; }
    public PostStatus Status { get; private set; } = PostStatus.Draft;
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public string? ExternalPostId { get; private set; }
    public string? FailureReason { get; private set; }
    public string[] Hashtags { get; private set; } = [];

    public Article Article { get; private set; }
    public Account Account { get; private set; }

    private Post() { }

    internal static Post Create(
        Guid articleId,
        Guid accountId,
        Platform platform,
        string content,
        DateTime? scheduledAt,
        string[] hashtags)
    {
        return new Post
        {
            ArticleId = articleId,
            AccountId = accountId,
            Platform = platform,
            Content = content,
            ScheduledAt = scheduledAt,
            Hashtags = hashtags,
            Status = scheduledAt.HasValue ? PostStatus.Scheduled : PostStatus.Pending
        };
    }

    public void MarkPublished(string externalPostId)
    {
        Status = PostStatus.Published;
        ExternalPostId = externalPostId;
        PostedAt = DateTime.UtcNow;
        Touch();
        AddDomainEvent(new PostPublishedEvent(Id, ArticleId, AccountId, Platform));
    }

    public void MarkFailed(string reason)
    {
        Status = PostStatus.Failed;
        FailureReason = reason;
        Touch();
    }

    public void MarkRejected()
    {
        Status = PostStatus.Rejected;
        Touch();
    }

    public void AttachVideo(string videoUrl)
    {
        VideoUrl = videoUrl;
        Touch();
    }

    public void Reschedule(DateTime scheduledAt)
    {
        ScheduledAt = scheduledAt;
        Status = PostStatus.Scheduled;
        Touch();
    }
}
