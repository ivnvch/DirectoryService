using FileService.Domain.Enums;

namespace FileService.Domain.Extensions;

public static class AssetTypeExtensions
{
    public static AssetType ToAssetType(this string value)
    {
        return value.ToLowerInvariant() switch
        {
            "video" => AssetType.Video,
            "preview" => AssetType.Preview,
            "avatar" => AssetType.Avatar,
            _ => throw new ArgumentException($"Invalid asset type: {value}")
        };
    }
}