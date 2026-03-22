using CSharpFunctionalExtensions;
using Shared.Errors;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Key { get; }
    public string Prefix { get; }
    public string Location { get; }
    private string Value { get; }
    public string FullPath { get; }

    private StorageKey(string key, string prefix, string location)
    {
        Location = location;
        Prefix = prefix;
        Key = key;
        Value = string.IsNullOrWhiteSpace(prefix) ? key : $"{prefix}/{key}";
        FullPath = $"{location}/{Value}";
    }
    
    public static StorageKey None => new StorageKey(string.Empty, string.Empty, string.Empty);

    public static Result<StorageKey, Error> Create(string location, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(location))
            return GeneralErrors.ValueIsInvalid(nameof(location));

        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;
        
        Result<string, Error> normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;

        return new StorageKey(location.Trim(), normalizedPrefixResult.Value, normalizedPrefixResult.Value);
    }

    public Result<StorageKey, Error> AppendSegment(string segment)
    {
        return Create(Location, Prefix, Key + "/" + segment);
    }

    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;
        
        string[] parts = prefix.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            Result<string, Error> normalizedPart = NormalizeSegment(part);
            
            if (normalizedPart.IsFailure)
                return normalizedPart;
            
            if (!string.IsNullOrEmpty(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }
        
        return string.Join('/', normalizedParts);
    }

    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid(nameof(value));

        string trimed = value.Trim();

        if (trimed.Contains('/', StringComparison.Ordinal) || trimed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid(nameof(trimed));

        return trimed;
    }
    
}