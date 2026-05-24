using Microsoft.AspNetCore.Identity;
using NewsFlow.Core.Common;

namespace NewsFlow.Core.Entities;

/// <summary>
/// Application user.  Extends IdentityUser&lt;Guid&gt; so that ASP.NET Core Identity
/// manages password hashing, lockout, claims, etc.  Domain-event infrastructure
/// from BaseEntity is inlined here because C# only allows single inheritance.
/// </summary>
public class User : IdentityUser<Guid>
{
    // ── Custom fields ────────────────────────────────────────────────────────
    public string Name { get; private set; } = string.Empty;
    public string Plan { get; private set; } = "Free";
    public bool IsActive { get; private set; } = true;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    // ── Navigation properties ────────────────────────────────────────────────
    private readonly List<Article> _articles = [];
    private readonly List<Account> _accounts = [];
    public IReadOnlyCollection<Article> Articles => _articles.AsReadOnly();
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    // ── Domain events (inlined from BaseEntity) ──────────────────────────────
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void Touch() => UpdatedAt = DateTime.UtcNow;

    // Required by EF Core
    private User() { }

    /// <summary>
    /// Creates a new User value object.  Password hashing is intentionally
    /// excluded — pass the returned user to <c>UserManager.CreateAsync(user, password)</c>
    /// which applies PBKDF2 hashing automatically.
    /// </summary>
    public static Result<User> Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>("Name is required.");
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>("Email is required.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,   // Identity uses UserName as the unique login key
        };

        return Result.Success(user);
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
        Touch();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        Touch();
    }

    public void Upgrade(string plan)
    {
        Plan = plan;
        Touch();
    }
}
