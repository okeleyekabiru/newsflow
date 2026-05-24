using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Events;

namespace NewsFlow.Core.Entities;

public class Article : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string ContentMd { get; private set; }
    public ArticleStatus Status { get; private set; } = ArticleStatus.Draft;
    public ArticleCategory Category { get; private set; } = ArticleCategory.General;
    public ArticleTemplate Template { get; private set; } = ArticleTemplate.BreakingNews;
    public string? SourceName { get; private set; }
    public string? SourceUrl { get; private set; }
    public int WordCount { get; private set; }

    private readonly List<ArticleVersion> _versions = [];
    private readonly List<Post> _posts = [];

    public IReadOnlyCollection<ArticleVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public User User { get; private set; }

    private Article() { }

    public static Result<Article> Create(
        Guid userId,
        string title,
        string contentMd,
        ArticleCategory category,
        ArticleTemplate template)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Article>("Title is required.");
        if (string.IsNullOrWhiteSpace(contentMd))
            return Result.Failure<Article>("Content is required.");

        var article = new Article
        {
            UserId = userId,
            Title = title.Trim(),
            ContentMd = contentMd,
            Category = category,
            Template = template,
            WordCount = CountWords(contentMd)
        };

        article.SaveVersion();
        return Result.Success(article);
    }

    public Result Update(string title, string contentMd, ArticleCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Title is required.");

        Title = title.Trim();
        ContentMd = contentMd;
        Category = category;
        WordCount = CountWords(contentMd);
        Touch();
        SaveVersion();
        return Result.Success();
    }

    public void Publish()
    {
        Status = ArticleStatus.Published;
        Touch();
        AddDomainEvent(new ArticlePublishedEvent(Id, UserId, Category));
    }

    public void Archive()
    {
        Status = ArticleStatus.Archived;
        Touch();
    }

    public void SetSource(string name, string url)
    {
        SourceName = name;
        SourceUrl = url;
        Touch();
    }

    private void SaveVersion()
    {
        var version = ArticleVersion.Create(Id, ContentMd, WordCount);
        _versions.Add(version);
    }

    private static int CountWords(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Trim().Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
}
