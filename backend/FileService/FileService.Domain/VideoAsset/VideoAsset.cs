using CSharpFunctionalExtensions;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Shared.CommonErrors;

namespace FileService.Domain.VideoAsset;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;
    
    public const string LOCATION = "videos";
    public const string ALLOWED_CONTENT_TYPE = "videos";
    
    public const string HLS_PREFIX = "hls";
    public const string RAW_PREFIX = "raw";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";

    public static readonly string[] AllowedExtensions = ["mp4", "nkv", "avi", "mov"];

    private VideoAsset(){}


    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key,
        StorageKey finalKey,
        StorageKey hlsRootKey)
        : base(id, mediaData, status, AssetType.Video, owner, key, finalKey)
    {
        HlsRootKey = hlsRootKey;
    }
    
    public StorageKey HlsRootKey { get; private set; }

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        UnitResult<Error> validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;
        
        Result<StorageKey, Error> key = StorageKey.Create(LOCATION, RAW_PREFIX, id.ToString());
        
        if (key.IsFailure)
            return key.Error;
        
        Result<StorageKey, Error> hlsKeyResult = StorageKey.Create(LOCATION, HLS_PREFIX, id.ToString());

        if (hlsKeyResult.IsFailure)
            return hlsKeyResult.Error;

        return new VideoAsset(
            id,
            mediaData,
            MediaStatus.Uploading,
            owner,
            key.Value,
            hlsKeyResult.Value,
            hlsKeyResult.Value);
    }

    public UnitResult<Error> CompleteProcessing(DateTime timestamp)
    {
        Result<StorageKey, Error> appendResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);
        
        if (appendResult.IsFailure)
            return appendResult.Error;
        
        HlsRootKey = appendResult.Value;

        UnitResult<Error> markResult = MarkReady(HlsRootKey, timestamp);

        if (markResult.IsFailure)
            return markResult;

        return UnitResult.Success<Error>();
    }
    
    private static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return Error.Validation("video.invalid.extension",
                $"File extension must be one of: {string.Join(". ", AllowedExtensions)}");

        if (mediaData.ContentType.Category != MediaType.Video)
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");

        if (mediaData.Size > MAX_SIZE)
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
        
        return UnitResult.Success<Error>();
    }
}