namespace FileService.Infrastructure.S3;

public record S3Options
{
    public string Endpoint { get; init; }
    public string AccessKey { get; init; }
    public string SecretKey { get; init; }
    public bool WithSsl { get; init; }
    public int DownloadUrlExpirationHours { get; init; } = 24;
    public IReadOnlyList<string> RequiredBuckets { get; init; } = [];
}