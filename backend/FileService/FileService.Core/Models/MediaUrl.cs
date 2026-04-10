using FileService.Domain.ValueObjects;

namespace FileService.Core.Models;

public record MediaUrl(StorageKey StorageKey, string PresignedUrl);