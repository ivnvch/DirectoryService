namespace FileService.Core.Features;

public record GetMediaAssetsRequest(IReadOnlyList<Guid> MediaAssetIds);