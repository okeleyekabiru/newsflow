using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// ElevenLabs text-to-speech provider.  Configure via appsettings:
/// <code>
/// "ElevenLabs": {
///   "ApiKey": "...",
///   "BaseUrl": "https://api.elevenlabs.io"   (optional, defaults shown)
/// }
/// </code>
/// </summary>
public class ElevenLabsVoiceProvider : IVoiceProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<ElevenLabsVoiceProvider> _logger;

    public ElevenLabsVoiceProvider(HttpClient http, IConfiguration config, ILogger<ElevenLabsVoiceProvider> logger)
    {
        _http   = http;
        _logger = logger;

        var apiKey  = config["ElevenLabs:ApiKey"] ?? throw new InvalidOperationException("ElevenLabs:ApiKey is not configured.");
        var baseUrl = config["ElevenLabs:BaseUrl"] ?? "https://api.elevenlabs.io";

        _http.BaseAddress = new Uri(baseUrl);
        _http.DefaultRequestHeaders.Add("xi-api-key", apiKey);
    }

    public async Task<byte[]> GenerateVoiceoverAsync(string script, string voiceId, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating voiceover with voice {VoiceId} ({Length} chars)", voiceId, script.Length);

        var payload = new
        {
            text           = script,
            model_id       = "eleven_multilingual_v2",
            voice_settings = new { stability = 0.5, similarity_boost = 0.75 }
        };

        var response = await _http.PostAsJsonAsync($"/v1/text-to-speech/{voiceId}", payload, ct);
        response.EnsureSuccessStatusCode();

        var audio = await response.Content.ReadAsByteArrayAsync(ct);
        _logger.LogInformation("Voiceover generated: {Bytes} bytes", audio.Length);
        return audio;
    }
}
