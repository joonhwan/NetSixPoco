using FileStore.Lib.Contracts;

namespace FileStore.Lib.Uploader;

public interface IMultiPartFormFileStreamingUploader
{
    Task<FileUploadResult> HandleAsync(string? baseDir = null, CancellationToken ct = default);
}