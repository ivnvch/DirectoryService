using FileService.Domain;

namespace FileService.Core;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsRead { get; }
}