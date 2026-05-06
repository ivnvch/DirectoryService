namespace FileService.Contracts.MediaAssets.Multipart.Requests;

public record GetChunkUploadUrlRequest(
    Guid MediaAssetId,
    string UploadId,
    int PartNumber);
