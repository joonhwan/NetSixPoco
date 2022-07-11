using Microsoft.Extensions.Logging;

namespace FileStore.Lib.Services;

internal class FileStoreClientServiceFactory : IFileStoreClientServiceFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly HttpClient _client;

    public FileStoreClientServiceFactory(HttpClient client, ILoggerFactory loggerFactory)
    {
        _client = client;
        _loggerFactory = loggerFactory;
    }

    public IFileStoreClientService CreateService(string baseUrlPath)
    {
        var logger = _loggerFactory.CreateLogger<FileStoreClientService>();
        return new FileStoreClientService(_client, baseUrlPath);
    }
}