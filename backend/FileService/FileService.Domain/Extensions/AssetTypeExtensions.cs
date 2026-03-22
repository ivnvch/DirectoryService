namespace FileService.Domain;

public static class AssetTypeExtensions
{
    public static AssetType ToAssetType(this string value)
    {
        return value switch
        {
            "video" => AssetType.Video,
            "preview" => AssetType.Preview,
            "avatar" => AssetType.Avatar,
            _ => throw new ArgumentException($"Invalid asset type: {value}")
        };
    }
}