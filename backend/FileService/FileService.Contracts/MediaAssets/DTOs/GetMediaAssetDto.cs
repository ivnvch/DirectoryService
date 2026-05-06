namespace FileService.Contracts.MediaAssets.DTOs;

public record GetMediaAssetDto(
    Guid Id,
    string Status,
    string AssetType,
    string? Url);
