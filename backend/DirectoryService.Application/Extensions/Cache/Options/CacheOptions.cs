namespace DirectoryService.Application.Extensions.Cache.Options;

public class CacheOptions
{
    public TimeSpan LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan Expiration { get; init; } = TimeSpan.FromMinutes(30);
}