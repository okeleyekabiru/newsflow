using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace NewsFlow.API.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthControllerTests(IntegrationTestFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidCredentials_Returns200AndTokens()
    {
        var email = $"register_{Guid.NewGuid():N}@test.com";

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name     = "New User",
            email,
            password = "Password1!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TokenBody>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });

        // Duplicate
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            name     = "Weak",
            email    = $"weak_{Guid.NewGuid():N}@test.com",
            password = "abc",   // too short / missing digit
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenBody>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns400()
    {
        var email = $"loginbad_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword99!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "nobody@nowhere.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithValidRefreshToken_Returns200AndNewTokens()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var regRes = await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });
        var tokens = await regRes.Content.ReadFromJsonAsync<TokenBody>();

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = tokens!.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await response.Content.ReadFromJsonAsync<TokenBody>();
        newTokens!.AccessToken.Should().NotBe(tokens.AccessToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = "not-a-real-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns200()
    {
        var email = $"logout_{Guid.NewGuid():N}@test.com";
        var regRes = await _client.PostAsJsonAsync("/api/auth/register",
            new { name = "User", email, password = "Password1!" });
        var tokens = await regRes.Content.ReadFromJsonAsync<TokenBody>();

        // Attach JWT
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var response = await _client.PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = tokens.RefreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _client.DefaultRequestHeaders.Authorization = null; // clean up
    }

    // ── Protected endpoint without token ─────────────────────────────────────

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/feed");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private sealed record TokenBody(string AccessToken, string RefreshToken);
}
