using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// FFmpeg-based video assembler.  Stitches downloaded audio + footage clips
/// into a final MP4 using a two-pass approach:
///   1. Concatenate footage clips → raw video
///   2. Mix in the generated audio track
///
/// Configure via appsettings:
/// <code>
/// "FFmpeg": {
///   "ExecutablePath": "ffmpeg",        // or full path, e.g. "C:/ffmpeg/bin/ffmpeg.exe"
///   "WorkDir": "C:/tmp/newsflow"       // scratch dir for intermediate files
/// }
/// </code>
/// </summary>
public class FfmpegVideoAssembler : IVideoAssembler
{
    private readonly string _ffmpeg;
    private readonly string _workDir;
    private readonly ILogger<FfmpegVideoAssembler> _logger;

    public FfmpegVideoAssembler(IConfiguration config, ILogger<FfmpegVideoAssembler> logger)
    {
        _ffmpeg  = config["FFmpeg:ExecutablePath"] ?? "ffmpeg";
        _workDir = config["FFmpeg:WorkDir"]        ?? Path.GetTempPath();
        _logger  = logger;

        Directory.CreateDirectory(_workDir);
    }

    public async Task<string> AssembleAsync(
        string scriptPath,
        string audioPath,
        IEnumerable<string> footageUrls,
        VideoFormat format,
        CancellationToken ct = default)
    {
        var jobId   = Guid.NewGuid().ToString("N");
        var jobDir  = Path.Combine(_workDir, jobId);
        Directory.CreateDirectory(jobDir);

        _logger.LogInformation("Assembling video job {JobId} ({Format})", jobId, format);

        try
        {
            // Download footage clips
            var clipPaths = await DownloadClipsAsync(footageUrls, jobDir, ct);

            // Build concat list
            var listPath = Path.Combine(jobDir, "clips.txt");
            await File.WriteAllLinesAsync(listPath, clipPaths.Select(p => $"file '{p}'"), ct);

            // Scale filter based on format
            var (w, h) = format switch
            {
                VideoFormat.YouTube_16x9                          => (1920, 1080),
                VideoFormat.TikTok_9x16 or VideoFormat.Reels_9x16
                    or VideoFormat.Shorts_9x16                    => (1080, 1920),
                _                                                 => (1080, 1920),
            };

            var rawVideo  = Path.Combine(jobDir, "raw.mp4");
            var outputMp4 = Path.Combine(jobDir, "output.mp4");

            // Step 1 — concatenate clips
            await RunFfmpegAsync(
                $"-f concat -safe 0 -i \"{listPath}\" -vf scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2 -c:v libx264 -an \"{rawVideo}\"",
                ct);

            // Step 2 — mix audio
            await RunFfmpegAsync(
                $"-i \"{rawVideo}\" -i \"{audioPath}\" -c:v copy -c:a aac -shortest \"{outputMp4}\"",
                ct);

            _logger.LogInformation("Video assembled: {Path}", outputMp4);
            return outputMp4;
        }
        catch
        {
            // Clean up scratch files on failure
            Directory.Delete(jobDir, recursive: true);
            throw;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<List<string>> DownloadClipsAsync(
        IEnumerable<string> urls, string dir, CancellationToken ct)
    {
        using var http  = new HttpClient();
        var paths = new List<string>();
        var i     = 0;

        foreach (var url in urls)
        {
            var dest = Path.Combine(dir, $"clip_{i++}.mp4");
            var data = await http.GetByteArrayAsync(url, ct);
            await File.WriteAllBytesAsync(dest, data, ct);
            paths.Add(dest);
        }

        return paths;
    }

    private async Task RunFfmpegAsync(string arguments, CancellationToken ct)
    {
        _logger.LogDebug("ffmpeg {Args}", arguments);

        var psi = new ProcessStartInfo(_ffmpeg, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ffmpeg process.");

        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"ffmpeg exited with code {process.ExitCode}: {stderr}");
    }
}
