using FileService.Domain;

namespace FileService.Core;

public interface IMediaRepository
{
    Task<MediaAsset?> GetByStoragePathAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken);
}
