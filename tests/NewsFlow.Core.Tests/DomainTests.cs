using FluentAssertions;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Infrastructure.ContentFilters;
using Xunit;

namespace NewsFlow.Core.Tests;

public class ArticleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = Article.Create(
            Guid.NewGuid(), "Test Title", "Test content here",
            ArticleCategory.Technology, ArticleTemplate.BreakingNews);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Test Title");
        result.Value.Category.Should().Be(ArticleCategory.Technology);
        result.Value.Status.Should().Be(ArticleStatus.Draft);
        result.Value.Versions.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldFail()
    {
        var result = Article.Create(
            Guid.NewGuid(), "", "Content", ArticleCategory.General, ArticleTemplate.BreakingNews);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Title");
    }

    [Fact]
    public void Update_ShouldSaveNewVersion()
    {
        var article = Article.Create(
            Guid.NewGuid(), "Original", "Original content",
            ArticleCategory.General, ArticleTemplate.BreakingNews).Value;

        article.Update("Updated Title", "Updated content here", ArticleCategory.Politics);

        article.Title.Should().Be("Updated Title");
        article.Versions.Should().HaveCount(2);
    }

    [Fact]
    public void Publish_ShouldRaiseDomainEvent()
    {
        var article = Article.Create(
            Guid.NewGuid(), "Title", "Content",
            ArticleCategory.General, ArticleTemplate.BreakingNews).Value;

        article.Publish();

        article.Status.Should().Be(ArticleStatus.Published);
        article.DomainEvents.Should().ContainSingle(e => e is Events.ArticlePublishedEvent);
    }
}

public class FlaggedPostTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = FlaggedPost.Create(
            Guid.NewGuid(), "Conflict detected", 6,
            ArticleCategory.ConflictAndWar,
            ["casualties", "airstrike"], "Reuters");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(FlagStatus.Pending);
        result.Value.SeverityScore.Should().Be(6);
    }

    [Fact]
    public void Create_WithInvalidSeverityScore_ShouldFail()
    {
        var result = FlaggedPost.Create(
            Guid.NewGuid(), "Reason", 15,
            ArticleCategory.ConflictAndWar, [], "Source");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Approve_ShouldTransitionStatus_AndAddAuditLog()
    {
        var flag = FlaggedPost.Create(
            Guid.NewGuid(), "Reason", 5,
            ArticleCategory.ConflictAndWar, [], "AP").Value;

        var reviewerId = Guid.NewGuid();
        flag.Approve(reviewerId, "Looks fine — trusted source.");

        flag.Status.Should().Be(FlagStatus.Approved);
        flag.ReviewedBy.Should().Be(reviewerId);
        flag.AuditLogs.Should().HaveCount(1);
        flag.AuditLogs.First().Action.Should().Be("Approved");
    }

    [Fact]
    public void Approve_AlreadyApproved_ShouldFail()
    {
        var flag = FlaggedPost.Create(
            Guid.NewGuid(), "Reason", 5,
            ArticleCategory.ConflictAndWar, [], "AP").Value;

        flag.Approve(Guid.NewGuid(), null);
        var secondApprove = flag.Approve(Guid.NewGuid(), null);

        secondApprove.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Escalate_ShouldRequireNotes()
    {
        var flag = FlaggedPost.Create(
            Guid.NewGuid(), "Reason", 9,
            ArticleCategory.Terrorism, [], "Unknown").Value;

        flag.Escalate(Guid.NewGuid(), "Needs senior review — graphic content.");

        flag.Status.Should().Be(FlagStatus.Escalated);
        flag.DomainEvents.Should().ContainSingle(e => e is Events.FlagEscalatedEvent);
    }
}

public class PostBuilderTests
{
    [Fact]
    public void Build_TwitterContentOverLimit_ShouldFail()
    {
        var article = Article.Create(
            Guid.NewGuid(),
            new string('A', 300),
            "Content", ArticleCategory.General, ArticleTemplate.BreakingNews).Value;

        var account = Account.Create(
            Guid.NewGuid(), Platform.Twitter, "@test", "token").Value;

        var result = new Common.PostBuilder()
            .ForArticle(article)
            .ForAccount(account)
            .Build();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("280");
    }

    [Fact]
    public void Build_ValidPost_ShouldSucceed()
    {
        var article = Article.Create(
            Guid.NewGuid(), "Short title", "Content",
            ArticleCategory.Technology, ArticleTemplate.BreakingNews).Value;

        var account = Account.Create(
            Guid.NewGuid(), Platform.TikTok, "@test", "token").Value;

        var result = new Common.PostBuilder()
            .ForArticle(article)
            .ForAccount(account)
            .WithHashtags("tech", "news")
            .Build();

        result.IsSuccess.Should().BeTrue();
        result.Value.Hashtags.Should().Contain("tech");
    }
}

public class ConflictFilterStrategyTests
{
    private readonly ConflictFilterStrategy _strategy = new();

    [Fact]
    public async Task Evaluate_NoConflictKeywords_ShouldAutoPost()
    {
        var article = Article.Create(
            Guid.NewGuid(), "NASDAQ hits record high",
            "Markets are up today.", ArticleCategory.Finance,
            ArticleTemplate.BreakingNews).Value;

        var result = await _strategy.EvaluateAsync(article, null);

        result.Decision.Should().Be(ContentDecision.AutoPost);
        result.SeverityScore.Should().Be(0);
    }

    [Fact]
    public async Task Evaluate_MultipleConflictKeywords_ShouldFlagForReview()
    {
        var article = Article.Create(
            Guid.NewGuid(),
            "Airstrike causes casualties in conflict zone",
            "Heavy shelling reported, troops advance on frontline.",
            ArticleCategory.ConflictAndWar,
            ArticleTemplate.BreakingNews).Value;

        var result = await _strategy.EvaluateAsync(article, null);

        result.Decision.Should().Be(ContentDecision.FlagForReview);
        result.SeverityScore.Should().BeGreaterThan(3);
        result.TriggerKeywords.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Evaluate_TrustedSourceBelowThreshold_WithAutoPostEnabled_ShouldAutoPost()
    {
        var article = Article.Create(
            Guid.NewGuid(), "Ceasefire agreed",
            "Reuters: A ceasefire has been agreed.",
            ArticleCategory.ConflictAndWar,
            ArticleTemplate.BreakingNews).Value;
        article.SetSource("Reuters", "https://reuters.com");

        var ruleConfig = FlagRuleConfig.CreateDefault(Guid.NewGuid(), ArticleCategory.ConflictAndWar);
        ruleConfig.Update(ContentDecision.FlagForReview,
            ["Reuters", "AP News"], [], 4, null, true);

        var result = await _strategy.EvaluateAsync(article, ruleConfig);

        result.Decision.Should().Be(ContentDecision.AutoPost);
    }
}
