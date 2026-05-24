using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Common;

public class PostBuilder
{
    private Guid _articleId;
    private Guid _accountId;
    private Platform _platform;
    private string _content = string.Empty;
    private DateTime? _scheduledAt;
    private readonly List<string> _hashtags = [];
    private string? _videoUrl;

    private static readonly Dictionary<Platform, int> CharLimits = new()
    {
        { Platform.Twitter, 280 },
        { Platform.TikTok, 2200 },
        { Platform.Instagram, 2200 },
        { Platform.YouTube, 5000 },
        { Platform.Facebook, 63206 }
    };

    public PostBuilder ForArticle(Article article)
    {
        _articleId = article.Id;
        _content = article.Title;
        return this;
    }

    public PostBuilder ForAccount(Account account)
    {
        _accountId = account.Id;
        _platform = account.Platform;
        return this;
    }

    public PostBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public PostBuilder WithSchedule(DateTime scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public PostBuilder WithHashtags(params string[] hashtags)
    {
        _hashtags.AddRange(hashtags.Select(h => h.TrimStart('#')));
        return this;
    }

    public PostBuilder WithVideo(string videoUrl)
    {
        _videoUrl = videoUrl;
        return this;
    }

    public Result<Post> Build()
    {
        if (_articleId == Guid.Empty) return Result.Failure<Post>("Article is required.");
        if (_accountId == Guid.Empty) return Result.Failure<Post>("Account is required.");
        if (string.IsNullOrWhiteSpace(_content)) return Result.Failure<Post>("Content is required.");

        var limit = CharLimits.GetValueOrDefault(_platform, int.MaxValue);
        var contentWithTags = _content + FormatHashtags();

        if (contentWithTags.Length > limit)
            return Result.Failure<Post>($"Content exceeds {_platform} limit of {limit} characters.");

        var post = Post.Create(
            _articleId,
            _accountId,
            _platform,
            contentWithTags,
            _scheduledAt,
            [.. _hashtags]);

        if (_videoUrl is not null)
            post.AttachVideo(_videoUrl);

        return Result.Success(post);
    }

    private string FormatHashtags() =>
        _hashtags.Count == 0
            ? string.Empty
            : "\n\n" + string.Join(" ", _hashtags.Select(h => $"#{h}"));
}
