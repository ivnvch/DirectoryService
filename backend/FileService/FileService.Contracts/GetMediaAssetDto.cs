namespace FileService.Core.Features;

public record GetMediaAssetDto(
    Guid Id,
    string Status,
    string AssetType,
    string? Url);