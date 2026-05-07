namespace FileService.Contracts.MediaAssets.Queries.Requests;

public record GetMediaAssetsRequest(IReadOnlyList<Guid> MediaAssetIds);
