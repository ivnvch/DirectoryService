using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace FileService.Core.Extensions;

public static class StoragePathExtensions
{
    public static Result<(string BucketName, string ObjectKey), Error> ParseStoragePath(this string path)
    {
        Result<(string Location, string? Prefix, string Key), Error> parts = path.ParseStorageKeyParts();
        if (parts.IsFailure)
            return parts.Error;

        (string location, string? prefix, string key) = parts.Value;
        string objectKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}/{key}";
        return (location, objectKey);
    }

    public static Result<(string Location, string? Prefix, string Key), Error> ParseStorageKeyParts(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Error.Validation("file.path", "File path is required.");

        string normalized = path
            .Trim()
            .Replace('\\', '/')
            .Trim('/');

        if (string.IsNullOrWhiteSpace(normalized))
            return Error.Validation("file.path", "File path is required.");

        string[] parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length < 2)
            return Error.Validation("file.path", "File path must include bucket and object key.");

        string location = parts[0];
        string key = parts[^1];
        string? prefix = parts.Length > 2
            ? string.Join('/', parts[1..^1])
            : null;

        if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(key))
            return Error.Validation("file.path", "File path must include bucket and object key.");

        return (location, prefix, key);
    }
}
