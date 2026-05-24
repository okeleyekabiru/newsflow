using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Events;

namespace NewsFlow.Core.Entities;

public class FlaggedPost : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public string FlagReason { get; private set; }
    public int SeverityScore { get; private set; }
    public ArticleCategory Category { get; private set; }
    public string[] TriggerKeywords { get; private set; } = [];
    public string SourceName { get; private set; }
    public FlagStatus Status { get; private set; } = FlagStatus.Pending;
    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }

    public Article Article { get; private set; }

    private readonly List<FlagAuditLog> _auditLogs = [];
    public IReadOnlyCollection<FlagAuditLog> AuditLogs => _auditLogs.AsReadOnly();

    private FlaggedPost() { }

    public static Result<FlaggedPost> Create(
        Guid articleId,
        string flagReason,
        int severityScore,
        ArticleCategory category,
        string[] triggerKeywords,
        string sourceName)
    {
        if (severityScore is < 1 or > 10)
            return Result.Failure<FlaggedPost>("Severity score must be between 1 and 10.");

        var flag = new FlaggedPost
        {
            ArticleId = articleId,
            FlagReason = flagReason,
            SeverityScore = severityScore,
            Category = category,
            TriggerKeywords = triggerKeywords,
            SourceName = sourceName
        };

        flag.AddDomainEvent(new PostFlaggedEvent(flag.Id, articleId, category, severityScore));
        return Result.Success(flag);
    }

    public Result Approve(Guid reviewerId, string? notes = null)
    {
        if (Status != FlagStatus.Pending)
            return Result.Failure("Only pending flags can be approved.");

        Status = FlagStatus.Approved;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        Touch();

        _auditLogs.Add(FlagAuditLog.Create(Id, "Approved", reviewerId, notes));
        AddDomainEvent(new FlagApprovedEvent(Id, ArticleId, reviewerId));
        return Result.Success();
    }

    public Result Reject(Guid reviewerId, string? notes = null)
    {
        if (Status != FlagStatus.Pending)
            return Result.Failure("Only pending flags can be rejected.");

        Status = FlagStatus.Rejected;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        Touch();

        _auditLogs.Add(FlagAuditLog.Create(Id, "Rejected", reviewerId, notes));
        AddDomainEvent(new FlagRejectedEvent(Id, ArticleId, reviewerId));
        return Result.Success();
    }

    public Result Escalate(Guid reviewerId, string notes)
    {
        if (Status != FlagStatus.Pending)
            return Result.Failure("Only pending flags can be escalated.");

        Status = FlagStatus.Escalated;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = notes;
        Touch();

        _auditLogs.Add(FlagAuditLog.Create(Id, "Escalated", reviewerId, notes));
        AddDomainEvent(new FlagEscalatedEvent(Id, ArticleId, reviewerId, notes));
        return Result.Success();
    }
}
