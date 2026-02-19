namespace DirectoryService.Application.Extensions.Cache;

public static class CacheKeyExtensions
{
    private const char Separator = ':';

    public static string ToCacheKey(this string prefix, params object[] segments)
    {
        if (segments.Length == 0)
            return prefix;

        return string.Join(Separator, segments.Prepend(prefix));
    }

    public static string ToCacheKey(this string prefix, Guid id)
        => $"{prefix}{Separator}{id}";

    public static string ToCacheKey(this string prefix, int page, int pageSize, Guid? id = null)
        => id.HasValue
            ? $"{prefix}{Separator}{id.Value}{Separator}{page}{Separator}{pageSize}"
            : $"{prefix}{Separator}{page}{Separator}{pageSize}";
}
