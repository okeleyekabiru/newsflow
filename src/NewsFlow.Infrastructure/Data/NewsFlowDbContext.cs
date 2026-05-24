using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsFlow.Core.Common;
using NewsFlow.Core.Entities;

namespace NewsFlow.Infrastructure.Data;

// IdentityDbContext<TUser, TRole, TKey> wires up all Identity tables
// (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, …)
// and exposes DbSet<User> Users via the base class.
public class NewsFlowDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public NewsFlowDbContext(DbContextOptions<NewsFlowDbContext> options) : base(options) { }

    // Users DbSet is inherited from IdentityDbContext — no need to redeclare it
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleVersion> ArticleVersions => Set<ArticleVersion>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<FlaggedPost> FlaggedPosts => Set<FlaggedPost>();
    public DbSet<FlagAuditLog> FlagAuditLogs => Set<FlagAuditLog>();
    public DbSet<FlagRuleConfig> FlagRuleConfigs => Set<FlagRuleConfig>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Analytics> Analytics => Set<Analytics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must be called first — Identity registers its own entity configurations here
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            // Id, Email, UserName, PasswordHash, SecurityStamp, etc. are already
            // configured by IdentityDbContext.  We only add our custom columns.
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Plan).HasDefaultValue("Free");
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(500);
            e.Property(x => x.ContentMd).IsRequired();
            e.Property(x => x.Category).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Template).HasConversion<string>();
            e.HasOne(x => x.User).WithMany(u => u.Articles)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Versions).WithOne(v => v.Article)
                .HasForeignKey(v => v.ArticleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArticleVersion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ContentMd).IsRequired();
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Handle).IsRequired().HasMaxLength(100);
            e.Property(x => x.Platform).HasConversion<string>();
            e.Property(x => x.AccessToken).IsRequired();
            e.HasOne(x => x.User).WithMany(u => u.Accounts)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Platform).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Hashtags).HasColumnType("text[]");
            e.HasOne(x => x.Article).WithMany(a => a.Posts)
                .HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Account).WithMany(a => a.Posts)
                .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FlaggedPost>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.TriggerKeywords).HasColumnType("text[]");
            e.HasOne(x => x.Article).WithMany()
                .HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.AuditLogs).WithOne()
                .HasForeignKey(x => x.FlaggedPostId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlagRuleConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasConversion<string>();
            e.Property(x => x.DefaultDecision).HasConversion<string>();
            e.Property(x => x.TrustedSources).HasColumnType("text[]");
            e.Property(x => x.BlockedKeywords).HasColumnType("text[]");
            e.HasIndex(x => new { x.UserId, x.Category }).IsUnique();
        });

        modelBuilder.Entity<Source>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Url).IsRequired();
        });

        modelBuilder.Entity<Analytics>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Revenue).HasPrecision(18, 4);
            e.HasOne(x => x.Post).WithMany()
                .HasForeignKey(x => x.PostId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        DispatchDomainEvents();
        return await base.SaveChangesAsync(ct);
    }

    private void DispatchDomainEvents()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        foreach (var entry in entities)
            entry.Entity.ClearDomainEvents();
    }
}
