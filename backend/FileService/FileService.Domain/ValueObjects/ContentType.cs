using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using Shared.CommonErrors;

namespace FileService.Domain.ValueObjects;

public sealed record ContentType
{
    public string Value { get; }
    public MediaType Category { get; }
    
    private ContentType(string value, MediaType category)
    {
        Value = value;
        Category = category;
    }

    public static Result<ContentType, Error> Create(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        MediaType category = contentType switch
        {
            _ when contentType.Contains("video", StringComparison.InvariantCultureIgnoreCase) => MediaType.Video,
            _ when contentType.Contains("image", StringComparison.InvariantCultureIgnoreCase) => MediaType.Image,
            _ when contentType.Contains("audio", StringComparison.InvariantCultureIgnoreCase) => MediaType.Audio,
            _ when contentType.Contains("document", StringComparison.InvariantCultureIgnoreCase) => MediaType.Document,
            _ => MediaType.Unknown
        };

        return new ContentType(contentType, category);
    }
    
}