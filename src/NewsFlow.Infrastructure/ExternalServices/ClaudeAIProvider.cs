using System.Net.Http.Json;
using System.Text.Json;
using NewsFlow.Core.Entities;
using NewsFlow.Core.Enums;
using NewsFlow.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsFlow.Infrastructure.ExternalServices;

public class ClaudeAIProvider : IAIProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<ClaudeAIProvider> _logger;
    private readonly string _model;
    private readonly string _apiKey;

    private static readonly Dictionary<Platform, int> PlatformLimits = new()
    {
        { Platform.Twitter, 280 },
        { Platform.TikTok, 2200 },
        { Platform.Instagram, 2200 },
        { Platform.YouTube, 5000 },
        { Platform.Facebook, 63206 }
    };

    public ClaudeAIProvider(
        HttpClient http,
        IConfiguration config,
        ILogger<ClaudeAIProvider> logger)
    {
        _http = http;
        _logger = logger;
        _apiKey = config["Anthropic:ApiKey"]!;
        _model = config["Anthropic:Model"] ?? "claude-sonnet-4-20250514";

        _http.BaseAddress = new Uri("https://api.anthropic.com");
        _http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> GenerateTextAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        var payload = new
        {
            model = _model,
            max_tokens = 1024,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        var response = await _http.PostAsJsonAsync("/v1/messages", payload, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
        return json!.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }

    public Task<string> RewriteHeadlineAsync(string headline, CancellationToken ct = default) =>
        GenerateTextAsync(
            "You are a professional news editor. Rewrite headlines to be more engaging " +
            "while keeping them factual and under 100 characters. Return only the headline.",
            headline, ct);

    public Task<string> GenerateCaptionAsync(
        string content,
        Platform platform,
        CancellationToken ct = default)
    {
        var limit = PlatformLimits.GetValueOrDefault(platform, 2200);
        return GenerateTextAsync(
            $"You are a social media expert. Write a compelling {platform} caption " +
            $"under {limit} characters. Include relevant hashtags. Return only the caption.",
            content, ct);
    }

    public Task<string> GenerateScriptAsync(Article article, CancellationToken ct = default) =>
        GenerateTextAsync(
            "You are a video scriptwriter. Convert this news article into a concise " +
            "60-90 second video script. Use clear, conversational language. " +
            "Format with [INTRO], [BODY], [OUTRO] sections.",
            $"Title: {article.Title}\n\n{article.ContentMd}", ct);
}
