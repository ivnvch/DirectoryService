namespace FileService.Contracts.MediaAssets.Multipart.Responses;

public record GetChunkUploadUrlResponse(string UploadUrl, int PartNumber);
