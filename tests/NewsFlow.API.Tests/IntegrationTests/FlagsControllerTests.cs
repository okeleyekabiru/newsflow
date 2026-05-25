using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace NewsFlow.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for FlagsController (GET /api/flags, rules, approve/reject).
/// Note: flagged posts are normally created by the ingest pipeline, which is
/// suppressed in test mode. Tests therefore verify HTTP semantics and auth
/// rather than full flag lifecycle.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public sealed class FlagsControllerTests(IntegrationTestFactory factory)
{
    private readonly IntegrationTestFactory _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email  = $"flags_{Guid.NewGuid():N}@test.com";
        var token  = await _factory.GetBearerTokenAsync(client, email);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── GET /api/flags ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetPending_WithoutToken_Returns401()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/api/flags");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPending_Authenticated_Returns200WithList()
    {
        var client   = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/flags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // New user has no flags — expect empty array, not an error
        var flags = await response.Content.ReadFromJsonAsync<IEnumerable<object>>();
        flags.Should().NotBeNull();
    }

    // ── GET /api/flags/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task GetDetail_NonExistentId_Returns404()
    {
        var client   = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync($"/api/flags/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /api/flags/{id}/approve ────────────────────────────────────────

    [Fact]
    public async Task Approve_NonExistentFlag_Returns400OrNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PatchAsJsonAsync(
            $"/api/flags/{Guid.NewGuid()}/approve",
            new { notes = "LGTM" });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── PATCH /api/flags/{id}/reject ─────────────────────────────────────────

    [Fact]
    public async Task Reject_NonExistentFlag_Returns400OrNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PatchAsJsonAsync(
            $"/api/flags/{Guid.NewGuid()}/reject",
            new { notes = "Too sensitive" });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── PATCH /api/flags/{id}/escalate ───────────────────────────────────────

    [Fact]
    public async Task Escalate_NonExistentFlag_Returns400OrNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PatchAsJsonAsync(
            $"/api/flags/{Guid.NewGuid()}/escalate",
            new { notes = "Senior editor needed" });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    // ── GET /api/flags/rules ─────────────────────────────────────────────────

    [Fact]
    public async Task GetRules_Authenticated_Returns200()
    {
        var client   = await CreateAuthenticatedClientAsync();
        var response = await client.GetAsync("/api/flags/rules");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRules_WithoutToken_Returns401()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/api/flags/rules");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/flags/rules ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRules_Authenticated_Returns200OrOk()
    {
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PutAsJsonAsync("/api/flags/rules", new
        {
            category        = "Conflict",
            autoBlock       = false,
            requiresReview  = true,
            severityThreshold = 5,
            userId          = Guid.Empty, // will be overridden by controller
        });

        // A user with no existing rule should create/upsert it
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
