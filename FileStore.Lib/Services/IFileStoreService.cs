using FileStore.Lib.Contracts;

namespace FileStore.Lib.Services;

public interface IFileStoreService : IBaseFileStoreService
{
    Task<FileUploadResult> UploadFilesAsync(string bucketId, CancellationToken ct = default);
}