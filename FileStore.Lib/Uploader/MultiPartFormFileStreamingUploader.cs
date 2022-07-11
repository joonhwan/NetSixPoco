using System.ComponentModel.DataAnnotations;
using System.Net;
using FileStore.Lib.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FileStore.Lib.Uploader;

public class MultiPartFormFileStreamingUploader : IMultiPartFormFileStreamingUploader
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IProgressEventNotifier _progressEventNotifier;
    private readonly ILogger<MultiPartFormFileStreamingUploader> _logger;
    private readonly FileStoreServiceOptions _uploaderOptions;
    
    public MultiPartFormFileStreamingUploader(
        IHttpContextAccessor contextAccessor, 
        IOptions<FileStoreServiceOptions> uploaderOptions,
        IProgressEventNotifier progressEventNotifier,
        ILogger<MultiPartFormFileStreamingUploader> logger
        )
    {
        _contextAccessor = contextAccessor;
        _progressEventNotifier = progressEventNotifier;
        _logger = logger;
        _uploaderOptions = uploaderOptions.Value;
    }
    
    public async Task<FileUploadResult> HandleAsync(string? baseDir = null, CancellationToken ct = default)
    {
        baseDir ??= _uploaderOptions.FallbackBaseDir;

        var request = _contextAccessor.HttpContext?.Request;
        if (request is null)
        {
            throw new InvalidOperationException("HttpContext 에서 Request 를 찾을 수 없습니다");
        }
        
        var isNormalMultipartContent =
            request.ContentType is not null &&
            MultipartRequestHelper.IsMultipartContentType(request.ContentType);
        if (!isNormalMultipartContent)
        {
            // ModelState.AddModelError("File", $"The request couldn't be processed (Error 1).");
            // // Log error
            //
            // return BadRequest(ModelState);
            throw new ValidationException("HTTP Request 의 ContentType 이 `multipart/*` 이 아니네요.");
        }

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(request.ContentType),
            _uploaderOptions.MultipartBoundaryLengthLimit
        );

        var result = new FileUploadResult();
        var reader = new MultipartReader(boundary, request.Body);
        var section = await reader.ReadNextSectionAsync(ct);
        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                // This check assumes that there's a file
                // present without form data. If form data
                // is present, this method immediately fails
                // and returns the model error.
                if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    throw new ValidationException(
                        "Request 에 FileContent 가 form-data 이면서 file 정보가 있어야 합니다"
                    );
                }

                // Don't trust the file name sent by the client. To display
                // the file name, HTML-encode the value.
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition!.FileName.Value);

                //
                // Streaming File 
                // 
                var bufferSize = 64 * 1024;
                var totalBytes = 0L;
                var requestId = string.Empty;
                if (section.Headers is not null)
                {
                    if(section.Headers.TryGetValue(HeaderNames.ContentLength, out var contentLengthStringValue))
                    {
                        if (long.TryParse(contentLengthStringValue, out var contentLength))
                        {
                            totalBytes = contentLength;
                        }
                    }

                    if (section.Headers.TryGetValue("X-Request-ID", out var requestIdValue))
                    {
                        requestId = requestIdValue;
                    }
                }
                
                var writtenBytes = 0;
                var bytes = new byte[bufferSize];
                var buffer = bytes.AsMemory();
                var targetFilePath = Path.Combine(baseDir, trustedFileNameForDisplay); //trustedFileNameForFileStorage);
                var tempFilePath = targetFilePath + ".uploading";
                
                _logger.LogInformation("Begin Uploading : RequestId={RequestId},  Length={TotalBytes:N}", 
                    requestId, totalBytes);

                // TODO FileLocking 
                var targetDir = Path.GetDirectoryName(targetFilePath)!;
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                
                var progress = 0.0F;
                await _progressEventNotifier.NotifyAsync(requestId, progress);
                
                await using var targetStream 
                    = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize, FileOptions.WriteThrough);
                while (true)
                {
                    var readBytes = await section.Body.ReadAsync(buffer, ct);
                    if (readBytes <= 0)
                    {
                        break;
                    }

                    await targetStream.WriteAsync(buffer[..readBytes], ct);
                    writtenBytes += readBytes;

                    if (!string.IsNullOrEmpty(requestId))
                    {
                        var currProgress = totalBytes > 0 ? writtenBytes * 10F / totalBytes : 0F;
                        if (currProgress >= progress + 5.0F)
                        {
                            progress = currProgress;
                            await _progressEventNotifier.NotifyAsync(requestId, progress);
                        }
                    }
                }

                // TODO FileLocking  
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }
                File.Move(tempFilePath, targetFilePath);
                // 

                var targetFileName = Path.GetFileName(targetFilePath);
                result.Entries.Add(new FileUploadEntry(contentDisposition.FileName.Value, targetFileName, writtenBytes, targetFilePath));
                
                await _progressEventNotifier.NotifyAsync(requestId, 100.0F);
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync(ct);
        }

        return result;
    }
}