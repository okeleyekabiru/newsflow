using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// Cloudflare R2 storage service (S3-compatible).
/// Configure via appsettings:
/// <code>
/// "Storage": {
///   "AccountId":    "...",          // used to build the R2 endpoint URL
///   "AccessKey":    "...",
///   "SecretKey":    "...",
///   "BucketName":   "newsflow-media",
///   "PublicDomain": "https://media.example.com"   // optional CDN domain
/// }
/// </code>
/// </summary>
public class R2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _publicDomain;
    private readonly ILogger<R2StorageService> _logger;

    public R2StorageService(IConfiguration config, ILogger<R2StorageService> logger)
    {
        _logger = logger;

        var accountId    = config["Storage:AccountId"] ?? throw new InvalidOperationException("Storage:AccountId is not configured.");
        var accessKey    = config["Storage:AccessKey"] ?? throw new InvalidOperationException("Storage:AccessKey is not configured.");
        var secretKey    = config["Storage:SecretKey"] ?? throw new InvalidOperationException("Storage:SecretKey is not configured.");
        _bucket          = config["Storage:BucketName"] ?? throw new InvalidOperationException("Storage:BucketName is not configured.");
        _publicDomain    = config["Storage:PublicDomain"] ?? $"https://{accountId}.r2.cloudflarestorage.com/{_bucket}";

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var s3Config    = new AmazonS3Config
        {
            ServiceURL     = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
        };

        _s3 = new AmazonS3Client(credentials, s3Config);
    }

    /// <summary>Uploads bytes to R2 and returns the public CDN URL.</summary>
    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

        using var stream = new MemoryStream(data);
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName  = _bucket,
            Key         = key,
            InputStream = stream,
            ContentType = contentType,
        }, ct);

        var url = $"{_publicDomain.TrimEnd('/')}/{key}";
        _logger.LogInformation("Uploaded {FileName} → {Url}", fileName, url);
        return url;
    }

    /// <inheritdoc/>
    public async Task<byte[]> DownloadAsync(string url, CancellationToken ct = default)
    {
        // Key is everything after the domain prefix
        var key = url.StartsWith(_publicDomain)
            ? url[(_publicDomain.TrimEnd('/').Length + 1)..]
            : new Uri(url).AbsolutePath.TrimStart('/');

        var response = await _s3.GetObjectAsync(
            new GetObjectRequest { BucketName = _bucket, Key = key }, ct);

        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var key = url.StartsWith(_publicDomain)
            ? url[(_publicDomain.TrimEnd('/').Length + 1)..]
            : new Uri(url).AbsolutePath.TrimStart('/');

        await _s3.DeleteObjectAsync(_bucket, key, ct);
        _logger.LogInformation("Deleted {Url} from R2", url);
    }
}
