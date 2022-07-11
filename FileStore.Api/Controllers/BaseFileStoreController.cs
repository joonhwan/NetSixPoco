using System.Net;
using System.Text.Encodings.Web;
using FileStore.Api.Utils;
using FileStore.Lib.Services;
using FileStore.Lib.Uploader;
using FileStore.Lib.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace FileStore.Api.Controllers;

[ApiController]
public abstract class BaseFileStoreController : ControllerBase 
{
    protected abstract IFileStoreService FileStoreService { get; set; }

    [HttpGet]   
    public async Task<IActionResult> GetBuckets()
    {
        var result = await FileStoreService.GetBucketsAsync();
        return Ok(result);
    }
    
    [HttpGet("{bucketId}")]
    public async Task<IActionResult> GetFiles(string bucketId)
    {
        var result = await FileStoreService.GetFilesAsync(bucketId);
        result.StoredFileNames = result.StoredFileNames.Select(GetUrlFromRelativePath).ToList();
        return Ok(result);
    }

    [HttpGet("{bucketId}/{fileName}")]
    public async Task<IActionResult> GetFile(string bucketId, string fileName)
    {
        fileName = WebUtility.UrlDecode(fileName);
        var info = await FileStoreService.GetFileInfoAsync(bucketId, fileName);
        return PhysicalFile(info.ServerFilePath, info.ContentType);
    }

    [HttpPost("{bucketId}")]
    [DisableFormValueModelBinding] // .NET5 이상에서는 필요없음.
    public async Task<IActionResult> UploadAsync(string bucketId, CancellationToken ct = default)
    {
        var result = await FileStoreService.UploadFilesAsync(bucketId, ct);
        return Ok(result);
    }

    protected string GetUrlFromRelativePath(string relativePath)
    {
        var request = HttpContext.Request;
        var encodedPath = WebUtility.UrlEncode(relativePath);
        return $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}/{encodedPath}";
    }
}