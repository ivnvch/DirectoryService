using FileService.Core;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;

namespace FileService.Infrastructure.Postgres.Repositiories;

public sealed class MediaRepository : IMediaRepository
{
    private readonly FileServiceDbContext _dbContext;

    public MediaRepository(FileServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MediaAsset?> GetByStoragePathAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken)
    {
        return _dbContext.MediaAssets
            .FirstOrDefaultAsync(
                asset =>
                    (asset.Key.Location == bucketName && asset.Key.Key == objectKey) ||
                    (asset.FinalKey.Location == bucketName && asset.FinalKey.Key == objectKey),
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public Task AddAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
