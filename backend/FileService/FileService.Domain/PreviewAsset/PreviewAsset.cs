using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.Errors;

namespace FileService.Domain;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 10_000_000;
    
    public const string ASSET_TYPE = "preview";
    public const string LOCATION = "preview";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "image";

    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];
    
    private PreviewAsset(){}
    
    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key,
        StorageKey finalKey)
        : base(id, mediaData, status, AssetType.Preview, owner, key, finalKey){}

    private static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return Error.Validation("preview.invalid.extension",
                $"File extension must be one of: {string.Join(". ", AllowedExtensions)}");

        if (mediaData.ContentType.Category != MediaType.Image)
            return Error.Validation("preview.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");

        if (mediaData.Size > MAX_SIZE)
            return Error.Validation("preview.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        
        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        UnitResult<Error> validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> key = StorageKey.Create(LOCATION, RAW_PREFIX, id.ToString());
        
        if (key.IsFailure)
            return key.Error;
        
        return new PreviewAsset(
            id,
            mediaData,
            MediaStatus.Uploading,
            owner,
            key.Value,
            key.Value);
    }
    
    public UnitResult<Error> CompleteUpload(DateTime timestamp)
    {
        UnitResult<Error> markUploadedResult = MarkUploaded(timestamp);

        if (markUploadedResult.IsFailure)
            return markUploadedResult;

        UnitResult<Error> markReadyResult = MarkReady(Key, timestamp);

        if (markReadyResult.IsFailure)
            return markReadyResult;

        return UnitResult.Success<Error>();
    }
}