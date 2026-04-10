namespace FileService.Core.Features;

public record GetMediaAssetsResponse(IReadOnlyList<GetMediaAssetDto> MediaAssets);