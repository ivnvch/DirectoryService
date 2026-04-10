namespace FileService.Contracts;

public record CompleteMultipartUploadDtoRequest(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<PartETagDto> PartETags);