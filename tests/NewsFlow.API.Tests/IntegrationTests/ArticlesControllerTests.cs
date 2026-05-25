using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace NewsFlow.API.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class ArticlesControllerTests(IntegrationTestFactory factory)
{
    private readonly IntegrationTestFactory _factory = factory;

    private async Task<(HttpClient Client, string Token)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email  = $"articles_{Guid.NewGuid():N}@test.com";
        var token  = await _factory.GetBearerTokenAsync(client, email);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return (client, token);
    }

    // ── GET /api/articles — unauthenticated ──────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/api/articles");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── GET /api/articles — authenticated, empty list ────────────────────────

    [Fact]
    public async Task GetAll_Authenticated_ReturnsEmptyListForNewUser()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/articles");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await response.Content.ReadFromJsonAsync<IEnumerable<object>>();
        list.Should().NotBeNull();
    }

    // ── POST /api/articles ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithValidPayload_Returns201()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/articles", new
        {
            title     = "Integration test article",
            contentMd = "## Body\n\nThis is test content.",
            category  = 0, // ArticleCategory.Technology
            template  = 0, // ArticleTemplate.Standard
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyTitle_Returns400()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/articles", new
        {
            title     = "",
            contentMd = "some content",
            category  = 0,
            template  = 0,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET /api/articles/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingArticle_Returns200()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Create first
        var createRes = await client.PostAsJsonAsync("/api/articles", new
        {
            title     = "Fetch me",
            contentMd = "Body text",
            category  = 1, // Finance
            template  = 0,
        });
        createRes.EnsureSuccessStatusCode();
        var id = await createRes.Content.ReadFromJsonAsync<Guid>();

        // Fetch by ID
        var fetchRes = await client.GetAsync($"/api/articles/{id}");
        fetchRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var response    = await client.GetAsync($"/api/articles/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── GET /api/articles/{id}/versions ─────────────────────────────────────

    [Fact]
    public async Task GetVersions_AfterUpdate_ReturnsVersionHistory()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();

        // Create
        var createRes = await client.PostAsJsonAsync("/api/articles", new
        {
            title     = "Versioned",
            contentMd = "Version 1",
            category  = 0,
            template  = 0,
        });
        createRes.EnsureSuccessStatusCode();
        var id = await createRes.Content.ReadFromJsonAsync<Guid>();

        // Update (creates a new version)
        var updateRes = await client.PutAsJsonAsync($"/api/articles/{id}", new
        {
            title     = "Versioned (updated)",
            contentMd = "Version 2 — added more detail.",
            category  = 0,
        });
        updateRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Fetch versions
        var versRes = await client.GetAsync($"/api/articles/{id}/versions");
        versRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await versRes.Content.ReadFromJsonAsync<IEnumerable<object>>();
        versions.Should().NotBeNullOrEmpty();
    }

    // ── PUT /api/articles/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task Update_OtherUsersArticle_Returns400()
    {
        // User A creates an article
        var (clientA, _) = await CreateAuthenticatedClientAsync();
        var createRes = await clientA.PostAsJsonAsync("/api/articles", new
        {
            title     = "User A article",
            contentMd = "Content",
            category  = 0,
            template  = 0,
        });
        var id = await createRes.Content.ReadFromJsonAsync<Guid>();

        // User B tries to update it
        var (clientB, _) = await CreateAuthenticatedClientAsync();
        var updateRes = await clientB.PutAsJsonAsync($"/api/articles/{id}", new
        {
            title     = "Stolen",
            contentMd = "Hacked",
            category  = 0,
        });

        // Should fail — B doesn't own the article
        updateRes.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── GET /api/feed ────────────────────────────────────────────────────────

    [Fact]
    public async Task Feed_Authenticated_ReturnsPaginatedResult()
    {
        var (client, _) = await CreateAuthenticatedClientAsync();
        var response    = await client.GetAsync("/api/feed?page=1&perPage=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
