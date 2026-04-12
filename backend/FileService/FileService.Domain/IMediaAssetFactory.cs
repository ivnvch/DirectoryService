using CSharpFunctionalExtensions;
using FileService.Domain.ValueObjects;
using Shared.CommonErrors;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<VideoAsset.VideoAsset, Error> CreateVideoForUpload(MediaData mediaData, MediaOwner owner);
    Result<PreviewAsset.PreviewAsset, Error> CreatePreviewForUpload(MediaData mediaData, MediaOwner owner);
}