using FileService.Contracts.MediaAssets.DTOs;

namespace FileService.Contracts.MediaAssets.Multipart.Requests;

public record CompleteMultipartUploadDtoRequest(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<PartETagDto> PartETags);
