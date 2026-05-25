using DotNet.Testcontainers.Builders;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsFlow.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace NewsFlow.API.Tests;

/// <summary>
/// Shared integration-test factory.
/// Spins up a PostgreSQL 16 Testcontainers instance, replaces the real
/// DbContext connection string, suppresses all hosted/background services,
/// and auto-applies EF migrations before the first test runs.
/// </summary>
public sealed class IntegrationTestFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("newsflow_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilCommandIsCompleted("pg_isready -U test"))
        .Build();

    // ── IAsyncLifetime ───────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Force the ASP.NET Core host to build, then apply migrations.
        _ = CreateClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NewsFlowDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    // ── WebApplicationFactory ────────────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // ── Replace PostgreSQL DbContext with the test container ──────
            var dbDescriptor = services
                .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<NewsFlowDbContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            services.AddDbContext<NewsFlowDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            // ── Remove hosted background services (IngestWorker, etc.) ────
            // so they don't fire during tests and cause spurious network calls.
            var hostedServiceDescriptors = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var d in hostedServiceDescriptors)
                services.Remove(d);

            // ── Replace Redis distributed cache with in-memory ────────────
            // (OAuthController uses IDistributedCache for PKCE state)
            var redisDescriptor = services
                .FirstOrDefault(d => d.ImplementationType?.Name?.Contains("RedisCache") == true);
            if (redisDescriptor is not null)
                services.Remove(redisDescriptor);

            services.AddDistributedMemoryCache();
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a fresh user and returns a valid Bearer token
    /// that can be attached to subsequent requests.
    /// </summary>
    public async Task<string> GetBearerTokenAsync(
        HttpClient client,
        string email   = "test@example.com",
        string password = "Password1!")
    {
        // Register
        var regRes = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name     = "Test User",
            email,
            password,
        });

        if (!regRes.IsSuccessStatusCode)
        {
            // Already registered — fall back to login
            var loginRes = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            loginRes.EnsureSuccessStatusCode();
            var loginBody = await loginRes.Content.ReadFromJsonAsync<TokenResponse>();
            return loginBody!.AccessToken;
        }

        var regBody = await regRes.Content.ReadFromJsonAsync<TokenResponse>();
        return regBody!.AccessToken;
    }

    private sealed record TokenResponse(string AccessToken, string RefreshToken);
}

/// <summary>
/// xUnit collection definition — all integration tests share one factory
/// (and therefore one container) to avoid the overhead of spinning up
/// a new Postgres instance per test class.
/// </summary>
[CollectionDefinition(IntegrationTestCollection.Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestFactory>
{
    public const string Name = "Integration tests";
}
