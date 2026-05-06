using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Queries.Requests;
using FileService.Contracts.MediaAssets.Queries.Responses;
using Shared.CommonErrors;

namespace FileService.Contracts;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssets(GetMediaAssetsRequest request,
        CancellationToken cancellationToken);
    
    Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssets(Guid mediaAssetId,
        CancellationToken cancellationToken);
    
    Task<Result<CheckMediaAssetExistsResponse, Errors>> CheckMediaAssetExists(Guid mediaAssetId,
        CancellationToken cancellationToken);
}