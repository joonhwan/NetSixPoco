using System.Net.Http.Json;
using System.Text.Encodings.Web;
using FileStore.Lib.Contracts;

namespace FileStore.Lib.Services;

internal class FileStoreClientService : IFileStoreClientService
{
    private readonly HttpClient _client;
    private readonly string _baseUrlPath;

    public FileStoreClientService(HttpClient client, string baseUrlPath)
    {
        _client = client;
        _baseUrlPath = baseUrlPath;
    }

    public async Task<BucketList> GetBucketsAsync()
    {
        var response = await _client.GetAsync(_baseUrlPath);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BucketList>()
                     ?? throw new InvalidCastException();
        return result;
    }

    public async Task<FileList> GetFilesAsync(string bucketId)
    {
        var response = await _client.GetAsync($"{_baseUrlPath}/{bucketId}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileList>()
            ?? throw new InvalidOperationException();
        return result;
    }

    public async Task<FileDownloadInfo> GetFileInfoAsync(string bucketId, string filePath)
    {
        filePath = UrlEncoder.Default.Encode(filePath);
        var response = await _client.GetAsync($"{_baseUrlPath}/{bucketId}?filePath={filePath}");
        var result = await response.Content.ReadFromJsonAsync<FileDownloadInfo>()
                     ?? throw new InvalidOperationException();
        return result;
    }

    public async Task<FileUploadResult> UploadFilesAsync(string bucketId, List<string> filePaths, CancellationToken ct = default)
    {
        const int bufferSize = 64 * 1024;
        var requestId = Guid.NewGuid().ToString("N");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrlPath}/{bucketId}");
        var formData = new MultipartFormDataContent();
        foreach (var filePath in filePaths)
        {
            var fileSizeBytes = new FileInfo(filePath).Length;
            var fileName = Path.GetFileName(filePath);
            var fileStream = File.OpenRead(filePath);
            
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.Add("X-Request-ID", requestId);
            fileContent.Headers.Add("Content-Length", fileSizeBytes.ToString());
            formData.Add(fileContent, "files", fileName);
        }
        request.Content = formData;
        var response = await _client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileUploadResult>(cancellationToken: ct)
               ?? throw new InvalidOperationException();
    }
}
