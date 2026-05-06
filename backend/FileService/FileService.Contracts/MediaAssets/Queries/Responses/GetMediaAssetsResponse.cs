using FileService.Contracts.MediaAssets.DTOs;

namespace FileService.Contracts.MediaAssets.Queries.Responses;

public record GetMediaAssetsResponse(IReadOnlyList<GetMediaAssetDto> MediaAssets);
