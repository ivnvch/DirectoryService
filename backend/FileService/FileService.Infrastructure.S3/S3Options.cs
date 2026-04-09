namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string Endpoint { get; init; }
    public string AccessKey { get; init; }
    public string SecretKey { get; init; }
    public bool WithSsl { get; init; }
    public int DownloadUrlExpirationHours { get; init; } = 24;
    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
    public double UploadUrlExpirationHours { get; init; } = 1;
    public int MaxConcurrentRequests { get; init; } = 20;
    
    public long RecommendedChunkSizeBytes { get; init; } = 100 * 1024 * 1024;// 100 MB
    
    public int MaxChunks { get; init; } = 10_000;
}