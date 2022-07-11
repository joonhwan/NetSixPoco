using FileStore.Lib.Uploader;
using FileStore.Lib.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FileStore.Lib.Services;

internal class FileStoreServiceFactory : IFileStoreServiceFactory
{
    private readonly IMultiPartFormFileStreamingUploader _uploader;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILoggerFactory _loggerFactory;

    public FileStoreServiceFactory(IMultiPartFormFileStreamingUploader uploader, IContentTypeService contentTypeService, ILoggerFactory loggerFactory)
    {
        _uploader = uploader;
        _contentTypeService = contentTypeService;
        _loggerFactory = loggerFactory;
    }

    public IFileStoreService CreateService(string baseDir)
    {
        var logger = _loggerFactory.CreateLogger<FileStoreService>();
        return new FileStoreService(_uploader, _contentTypeService, logger)
        {
            BaseDir = baseDir
        };
    }
}