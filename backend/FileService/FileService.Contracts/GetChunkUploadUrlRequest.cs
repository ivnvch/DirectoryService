namespace FileService.Contracts;

public record GetChunkUploadUrlRequest(
    Guid MediaAssetId,
    string UploadId,
    int PartNumber);