using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.CommonErrors;

namespace FileService.Domain;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;
    
    public AssetType AssetType { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey Key { get; protected set; } = null!;

    public StorageKey FinalKey { get; protected set; } = null!;

    public MediaOwner Owner { get; protected set; } = null!;
    
    public MediaStatus Status { get; protected set; }
    
    protected MediaAsset()
    {}

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        MediaOwner owner,
        StorageKey key,
        StorageKey finalKey)
    {
        Id = id;
        MediaData = mediaData;
        AssetType = assetType;
        Status = status;
        Owner = owner;
        Key = key;
        FinalKey = finalKey;
    }

    public static Result<MediaAsset, Error> CreateForUpload(Guid mediaAssetId, MediaData mediaData, AssetType assetType, MediaOwner owner)
    {
        switch (assetType)
        {
            case AssetType.Video:
                Result<VideoAsset.VideoAsset, Error> videoAssetResult = VideoAsset.VideoAsset.CreateForUpload(mediaAssetId, mediaData, owner);
                    return videoAssetResult.IsFailure ? videoAssetResult.Error : videoAssetResult.Value;
            case AssetType.Preview:
                Result<PreviewAsset.PreviewAsset, Error> previewAssetResult = PreviewAsset.PreviewAsset.CreateForUpload(mediaAssetId, mediaData, owner);
                return previewAssetResult.IsFailure ? previewAssetResult.Error : previewAssetResult.Value;
            /*case AssetType.Avatar*/
            default: throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
        }
    }

    public UnitResult<Error> MarkUploaded(DateTime updatedAt)
    {
        if (Status is MediaStatus.Uploading)
        {
            Status = MediaStatus.Uploaded;
            UpdatedAt = updatedAt;
        
            return UnitResult.Success<Error>();
        }
        
        return GeneralErrors.ValueIsInvalid("mediaAsset uploading");
    }
    
    public UnitResult<Error> MarkReady(StorageKey finalKey, DateTime updatedAt)
    {
        if (Status is MediaStatus.Uploaded)
        {
            FinalKey = finalKey;
            Status = MediaStatus.Ready;
            UpdatedAt = updatedAt;
        
            return UnitResult.Success<Error>();
        }
        
        return GeneralErrors.ValueIsInvalid("mediaAsset uploaded");
    }

    public UnitResult<Error> MarkFailed(DateTime date)
    {
        if (Status is MediaStatus.Uploaded or MediaStatus.Uploading)
        {
            Status = MediaStatus.Failed;
            UpdatedAt = date;
            return UnitResult.Success<Error>();
        }

        return GeneralErrors.ValueIsInvalid("mediaAsset markFailed");
    }

    public UnitResult<Error> MarkDelete(DateTime deletedAt)
    {
        if (Status is not (MediaStatus.Uploaded or MediaStatus.Ready or MediaStatus.Failed))
            return Error.Validation("mediaAsset.status", "Media asset can be marked as deleted only if its status is ready, failed or uploaded");
        
        UpdatedAt = deletedAt;
        Status = MediaStatus.Deleted;

        return UnitResult.Success<Error>();
    }
}