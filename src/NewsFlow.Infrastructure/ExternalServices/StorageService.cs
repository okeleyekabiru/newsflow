using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsFlow.Core.Interfaces;

namespace NewsFlow.Infrastructure.ExternalServices;

/// <summary>
/// AWS S3 storage service.  Configure via appsettings:
/// <code>
/// "AWS": { "Region": "us-east-1" }
/// "Storage": { "BucketName": "newsflow-assets" }
/// </code>
/// Credentials are resolved from the standard AWS chain
/// (env vars → ~/.aws/credentials → IAM role).
/// </summary>
public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3, IConfiguration config, ILogger<S3StorageService> logger)
    {
        _s3     = s3;
        _bucket = config["Storage:BucketName"] ?? throw new InvalidOperationException("Storage:BucketName is not configured.");
        _logger = logger;
    }

    public async Task<string> UploadAsync(byte[] data, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

        using var stream = new MemoryStream(data);
        var request = new PutObjectRequest
        {
            BucketName  = _bucket,
            Key         = key,
            InputStream = stream,
            ContentType = contentType,
        };

        await _s3.PutObjectAsync(request, ct);

        var url = $"https://{_bucket}.s3.amazonaws.com/{key}";
        _logger.LogInformation("Uploaded {FileName} → {Url}", fileName, url);
        return url;
    }

    public async Task<byte[]> DownloadAsync(string url, CancellationToken ct = default)
    {
        var key = new Uri(url).AbsolutePath.TrimStart('/');

        var request  = new GetObjectRequest { BucketName = _bucket, Key = key };
        var response = await _s3.GetObjectAsync(request, ct);

        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var key = new Uri(url).AbsolutePath.TrimStart('/');
        await _s3.DeleteObjectAsync(_bucket, key, ct);
        _logger.LogInformation("Deleted {Url} from S3", url);
    }
}
