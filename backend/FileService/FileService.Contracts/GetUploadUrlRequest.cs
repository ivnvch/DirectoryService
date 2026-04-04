namespace FileService.Contracts;

public record GetUploadUrlRequest(string FileName, string ContentType, long FileSize, string Context, string AssetType);