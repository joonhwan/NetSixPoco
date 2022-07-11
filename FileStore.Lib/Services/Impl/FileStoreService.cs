using FileStore.Lib.Contracts;
using FileStore.Lib.Uploader;
using FileStore.Lib.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FileStore.Lib.Services;

internal class FileStoreService : IFileStoreService
{
    private readonly IMultiPartFormFileStreamingUploader _uploader;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<FileStoreService> _logger;

    public FileStoreService(IMultiPartFormFileStreamingUploader uploader,
        IContentTypeService contentTypeService, ILogger<FileStoreService> logger)
    {
        _uploader = uploader;
        _contentTypeService = contentTypeService;
        _logger = logger;
        BaseDir = Environment.CurrentDirectory;
    }

    public string BaseDir { get; set; }

    public Task<BucketList> GetBucketsAsync()
    {
        var result = new BucketList();
        if (Directory.Exists(BaseDir))
        {
            result.BucketNames = Directory
                    .EnumerateDirectories(BaseDir, "*", SearchOption.TopDirectoryOnly)
                    .Select(x => Path.GetRelativePath(BaseDir, x))
                    .ToList()
                ;
        }
        return Task.FromResult(result);
    }

    public Task<FileList> GetFilesAsync(string bucketId)
    {
        var dir = Path.Combine(BaseDir, bucketId);
        var result = new FileList();
        if (Directory.Exists(dir))
        {
            result.StoredFileNames = Directory
                .EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly)
                .Where(x =>
                {
                    var attr = new FileInfo(x).Attributes;
                    return !attr.HasFlag(FileAttributes.Hidden);
                })
                .Select(x => Path.GetRelativePath(dir, x))
                .ToList();
            ;
        }

        return Task.FromResult(result);
    }

    public async Task<FileUploadResult> UploadFilesAsync(string bucketId, CancellationToken ct = default)
    {
        var serverTargetDir = Path.Combine(BaseDir, bucketId);

        var result = await _uploader.HandleAsync(serverTargetDir, ct);
        var tiffFiles = result.Entries.Where(x =>
        {
            var ext = Path.GetExtension(x.RequestedFileName).ToLower();
            return ext is ".tif" or ".tiff";
        });
        foreach (var tiffFile in tiffFiles)
        {
            _logger.LogInformation(" --> Splitting {TiffFileName} into *.jpg images", tiffFile.StoredFilePath);
        }

        // TODO result 내 Tiff 파일 Entry를 쪼개진 파일목록으로 대치?
        return result;
    }

    public Task<FileDownloadInfo> GetFileInfoAsync(string bucketId, string filePath)
    {
        var serverFilePath = Path.Combine(BaseDir, bucketId, filePath);
        var contentType = _contentTypeService.Map(Path.GetFileName(filePath));
        return Task.FromResult(new FileDownloadInfo
        {
            ContentType = contentType,
            ServerFilePath = serverFilePath
        });
    }
}