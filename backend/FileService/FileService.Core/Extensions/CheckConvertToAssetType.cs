using FileService.Domain.Enums;

namespace FileService.Core.Extensions;

public static class CheckConvertToAssetType
{
    public static bool BeSupportedAssetType(string? value) =>
        Enum.TryParse<AssetType>(value, ignoreCase: true, out var assetType) &&
        assetType is AssetType.Video or AssetType.Preview;
}