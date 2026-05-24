using NewsFlow.Core.Common;

namespace NewsFlow.Core.Entities;

public class ArticleVersion : BaseEntity
{
    public Guid ArticleId { get; private set; }
    public string ContentMd { get; private set; }
    public int WordCount { get; private set; }
    public DateTime SavedAt { get; private set; }

    public Article Article { get; private set; }

    private ArticleVersion() { }

    public static ArticleVersion Create(Guid articleId, string contentMd, int wordCount)
    {
        return new ArticleVersion
        {
            ArticleId = articleId,
            ContentMd = contentMd,
            WordCount = wordCount,
            SavedAt = DateTime.UtcNow
        };
    }
}
