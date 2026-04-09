namespace FileService.Contracts;

public record GetChunkUploadUrlResponse(string UploadUrl, int PartNumber);