using NewsFlow.Core.Interfaces;

namespace NewsFlow.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IArticleRepository Articles { get; }
    IPostRepository Posts { get; }
    IAccountRepository Accounts { get; }
    IFlaggedPostRepository FlaggedPosts { get; }
    IFlagRuleConfigRepository FlagRuleConfigs { get; }
    ISourceRepository Sources { get; }
    IAnalyticsRepository Analytics { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
}
