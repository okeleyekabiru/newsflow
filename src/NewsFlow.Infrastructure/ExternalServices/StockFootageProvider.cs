using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// Pexels stock footage and image provider.  Configure via appsettings:
/// <code>
/// "Pexels": { "ApiKey": "..." }
/// </code>
/// </summary>
public class PexelsFootageProvider : IStockFootageProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<PexelsFootageProvider> _logger;

    public PexelsFootageProvider(HttpClient http, IConfiguration config, ILogger<PexelsFootageProvider> logger)
    {
        _http   = http;
        _logger = logger;

        var apiKey = config["Pexels:ApiKey"] ?? throw new InvalidOperationException("Pexels:ApiKey is not configured.");
        _http.BaseAddress = new Uri("https://api.pexels.com");
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(apiKey);
    }

    public async Task<IEnumerable<string>> SearchVideosAsync(string query, int count = 5, CancellationToken ct = default)
    {
        _logger.LogInformation("Searching Pexels videos: {Query} (count={Count})", query, count);

        var response = await _http.GetAsync($"/videos/search?query={Uri.EscapeDataString(query)}&per_page={count}", ct);
        response.EnsureSuccessStatusCode();

        using var doc  = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var urls = doc.RootElement
            .GetProperty("videos")
            .EnumerateArray()
            .SelectMany(v => v.GetProperty("video_files").EnumerateArray())
            .Where(f => f.GetProperty("quality").GetString() == "hd")
            .Select(f => f.GetProperty("link").GetString()!)
            .Take(count)
            .ToList();

        _logger.LogInformation("Found {Count} video URLs", urls.Count);
        return urls;
    }

    public async Task<IEnumerable<string>> SearchImagesAsync(string query, int count = 10, CancellationToken ct = default)
    {
        _logger.LogInformation("Searching Pexels images: {Query} (count={Count})", query, count);

        var response = await _http.GetAsync($"/v1/search?query={Uri.EscapeDataString(query)}&per_page={count}", ct);
        response.EnsureSuccessStatusCode();

        using var doc  = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var urls = doc.RootElement
            .GetProperty("photos")
            .EnumerateArray()
            .Select(p => p.GetProperty("src").GetProperty("large").GetString()!)
            .Take(count)
            .ToList();

        _logger.LogInformation("Found {Count} image URLs", urls.Count);
        return urls;
    }
}
