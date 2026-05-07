using Microsoft.AspNetCore.Http;

namespace FileService.Contracts.MediaAssets.Upload.Requests;

public record UploadFileRequest(IFormFile File, string Context, string AssetType);
