using FileService.Contracts.MediaAssets.DTOs;

namespace FileService.Contracts.MediaAssets.Multipart.Responses;

public record StartMultipartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    int ChunkSize);
