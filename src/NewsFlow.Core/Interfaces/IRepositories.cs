using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
}

public interface IArticleRepository : IRepository<Article>
{
    Task<IEnumerable<Article>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Article?> GetWithVersionsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Article>> GetByCategoryAsync(ArticleCategory category, CancellationToken ct = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
}

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetScheduledBeforeAsync(DateTime scheduledAt, CancellationToken ct = default);
    Task<IEnumerable<Post>> GetByArticleIdAsync(Guid articleId, CancellationToken ct = default);
    Task<IEnumerable<Post>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Account?> GetByPlatformAsync(Guid userId, Platform platform, CancellationToken ct = default);
}

public interface IFlaggedPostRepository : IRepository<FlaggedPost>
{
    Task<IEnumerable<FlaggedPost>> GetPendingAsync(CancellationToken ct = default);
    Task<IEnumerable<FlaggedPost>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<FlaggedPost?> GetWithAuditLogsAsync(Guid id, CancellationToken ct = default);
}

public interface IFlagRuleConfigRepository : IRepository<FlagRuleConfig>
{
    Task<FlagRuleConfig?> GetByUserAndCategoryAsync(Guid userId, ArticleCategory category, CancellationToken ct = default);
    Task<IEnumerable<FlagRuleConfig>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}

public interface ISourceRepository : IRepository<Source>
{
    Task<IEnumerable<Source>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Source>> GetAllActiveAsync(CancellationToken ct = default);
}

public interface IAnalyticsRepository : IRepository<Analytics>
{
    Task<IEnumerable<Analytics>> GetByPostIdAsync(Guid postId, CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default);
}
