using NewsFlow.Core.Common;
using NewsFlow.Core.Enums;

namespace NewsFlow.Core.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; private set; }
    public Platform Platform { get; private set; }
    public string Handle { get; private set; }
    public string AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? TokenExpiry { get; private set; }
    public bool IsActive { get; private set; } = true;
    public long FollowerCount { get; private set; }
    public decimal MonthlyRevenue { get; private set; }
    public int PostsPerDay { get; private set; }

    public User User { get; private set; }

    private readonly List<Post> _posts = [];
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    private Account() { }

    public static Result<Account> Create(
        Guid userId,
        Platform platform,
        string handle,
        string accessToken)
    {
        if (string.IsNullOrWhiteSpace(handle))
            return Result.Failure<Account>("Handle is required.");
        if (string.IsNullOrWhiteSpace(accessToken))
            return Result.Failure<Account>("Access token is required.");

        return Result.Success(new Account
        {
            UserId = userId,
            Platform = platform,
            Handle = handle.TrimStart('@'),
            AccessToken = accessToken
        });
    }

    public void UpdateTokens(string accessToken, string? refreshToken, DateTime? expiry)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiry = expiry;
        Touch();
    }

    public void UpdateStats(long followers, decimal revenue)
    {
        FollowerCount = followers;
        MonthlyRevenue = revenue;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
