using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace FileService.Core.FileStorage;

public interface IChunkSizeCalculator
{
    Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(
        long fileSiz);
}