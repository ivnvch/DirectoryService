namespace FileService.Contracts;

public record GetMediaAssetsResponse(IReadOnlyList<GetMediaAssetDto> MediaAssets);