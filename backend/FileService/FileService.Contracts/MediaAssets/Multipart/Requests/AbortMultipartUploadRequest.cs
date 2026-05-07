namespace FileService.Contracts.MediaAssets.Multipart.Requests;

public record AbortMultipartUploadRequest(
    Guid MediaAssetId, 
    string UploadId);
