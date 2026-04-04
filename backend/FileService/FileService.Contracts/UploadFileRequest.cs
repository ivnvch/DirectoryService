using Microsoft.AspNetCore.Http;

namespace FileService.Contracts;

public record UploadFileRequest(IFormFile File, string Context, string AssetType);