using Microsoft.EntityFrameworkCore;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;
using NewsFlow.Infrastructure.Data;

namespace NewsFlow.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly NewsFlowDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(NewsFlowDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);
    public void Remove(T entity) => Set.Remove(entity);
}

public class ArticleRepository : Repository<Article>, IArticleRepository
{
    public ArticleRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<Article>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(a => a.UserId == userId)
                 .OrderByDescending(a => a.UpdatedAt)
                 .ToListAsync(ct);

    public Task<Article?> GetWithVersionsAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(a => a.Versions).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Article>> GetByCategoryAsync(ArticleCategory category, CancellationToken ct = default) =>
        await Set.Where(a => a.Category == category).ToListAsync(ct);

    public Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default) =>
        Set.AnyAsync(a => a.Title == title, ct);
}

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<Post>> GetScheduledBeforeAsync(DateTime scheduledAt, CancellationToken ct = default) =>
        await Set.Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= scheduledAt)
                 .ToListAsync(ct);

    public async Task<IEnumerable<Post>> GetByArticleIdAsync(Guid articleId, CancellationToken ct = default) =>
        await Set.Where(p => p.ArticleId == articleId).ToListAsync(ct);

    public async Task<IEnumerable<Post>> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default) =>
        await Set.Where(p => p.AccountId == accountId).ToListAsync(ct);
}

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(a => a.UserId == userId && a.IsActive).ToListAsync(ct);

    public Task<Account?> GetByPlatformAsync(Guid userId, Platform platform, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(a => a.UserId == userId && a.Platform == platform && a.IsActive, ct);
}

public class FlaggedPostRepository : Repository<FlaggedPost>, IFlaggedPostRepository
{
    public FlaggedPostRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<FlaggedPost>> GetPendingAsync(CancellationToken ct = default) =>
        await Set.Include(f => f.Article)
                 .Where(f => f.Status == FlagStatus.Pending)
                 .OrderByDescending(f => f.SeverityScore)
                 .ToListAsync(ct);

    public async Task<IEnumerable<FlaggedPost>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Include(f => f.Article)
                 .Where(f => f.Article.UserId == userId)
                 .OrderByDescending(f => f.CreatedAt)
                 .ToListAsync(ct);

    public Task<FlaggedPost?> GetWithAuditLogsAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(f => f.Article)
           .Include(f => f.AuditLogs)
           .FirstOrDefaultAsync(f => f.Id == id, ct);
}

public class FlagRuleConfigRepository : Repository<FlagRuleConfig>, IFlagRuleConfigRepository
{
    public FlagRuleConfigRepository(NewsFlowDbContext db) : base(db) { }

    public Task<FlagRuleConfig?> GetByUserAndCategoryAsync(
        Guid userId, ArticleCategory category, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(r => r.UserId == userId && r.Category == category, ct);

    public async Task<IEnumerable<FlagRuleConfig>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(r => r.UserId == userId).ToListAsync(ct);
}

public class SourceRepository : Repository<Source>, ISourceRepository
{
    public SourceRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<Source>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await Set.Where(s => s.UserId == userId && s.IsActive).ToListAsync(ct);

    public async Task<IEnumerable<Source>> GetAllActiveAsync(CancellationToken ct = default) =>
        await Set.Where(s => s.IsActive).ToListAsync(ct);
}

public class AnalyticsRepository : Repository<Analytics>, IAnalyticsRepository
{
    public AnalyticsRepository(NewsFlowDbContext db) : base(db) { }

    public async Task<IEnumerable<Analytics>> GetByPostIdAsync(Guid postId, CancellationToken ct = default) =>
        await Set.Where(a => a.PostId == postId).ToListAsync(ct);

    public async Task<decimal> GetTotalRevenueAsync(
        Guid userId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await Set
            .Where(a => a.RecordedAt >= from && a.RecordedAt <= to &&
                        a.Post.Account.UserId == userId)
            .SumAsync(a => a.Revenue, ct);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly NewsFlowDbContext _db;

    public IArticleRepository Articles { get; }
    public IPostRepository Posts { get; }
    public IAccountRepository Accounts { get; }
    public IFlaggedPostRepository FlaggedPosts { get; }
    public IFlagRuleConfigRepository FlagRuleConfigs { get; }
    public ISourceRepository Sources { get; }
    public IAnalyticsRepository Analytics { get; }

    public UnitOfWork(NewsFlowDbContext db)
    {
        _db = db;
        Articles = new ArticleRepository(db);
        Posts = new PostRepository(db);
        Accounts = new AccountRepository(db);
        FlaggedPosts = new FlaggedPostRepository(db);
        FlagRuleConfigs = new FlagRuleConfigRepository(db);
        Sources = new SourceRepository(db);
        Analytics = new AnalyticsRepository(db);
    }

    public Task<int> CommitAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    public void Dispose() => _db.Dispose();
}
