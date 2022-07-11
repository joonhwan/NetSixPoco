using System.Globalization;
using System.Net;
using System.Text;
using FileStore.Api.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using FileStore.Lib.Uploader;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

// using SampleApp.Data;
// using SampleApp.Filters;
// using SampleApp.Models;
// using SampleApp.Utilities;

namespace FileStore.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly long _fileSizeLimit;
    private readonly ILogger<StreamingController> _logger;
    private readonly IMultiPartFormFileStreamingUploader _streamingUploader;
    private readonly string[] _permittedExtensions = { ".txt" };
    private readonly string _targetFilePath;

    // Get the default form options so that we can use them to set the default 
    // limits for request body data.
    private static readonly FormOptions DefaultFormOptions = new FormOptions();

    public StreamingController(ILogger<StreamingController> logger, IConfiguration config, IHostEnvironment environment, IMultiPartFormFileStreamingUploader streamingUploader)
    {
        _logger = logger;
        _streamingUploader = streamingUploader;
        _fileSizeLimit = config.GetValue<long>("FileSizeLimit");

        // To save physical files to a path provided by configuration:
        // _targetFilePath = config.GetValue<string>("StoredFilesPath");
        _targetFilePath = Path.Combine(environment.ContentRootPath, "Store");

        // To save physical files to the temporary files folder, use:
        //_targetFilePath = Path.GetTempPath();
    }

    // The following upload methods:
    //
    // 1. Disable the form value model binding to take control of handling 
    //    potentially large files.
    //
    // 2. Typically, antiforgery tokens are sent in request body. Since we 
    //    don't want to read the request body early, the tokens are sent via 
    //    headers. The antiforgery token filter first looks for tokens in 
    //    the request header and then falls back to reading the body.

    #region snippet_UploadDatabase

    [HttpPost("upload-db")]
    [DisableFormValueModelBinding]
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDatabase()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File",
                $"The request couldn't be processed (Error 1).");
            // Log error

            return BadRequest(ModelState);
        }

        // Accumulate the form data key-value pairs in the request (formAccumulator).
        var formAccumulator = new KeyValueAccumulator();
        var streamedFileContent = Array.Empty<byte>();

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            DefaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper
                    .HasFileContentDisposition(contentDisposition))
                {
                    var untrustedFileNameForStorage = contentDisposition!.FileName.Value;
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    var trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

                    var stream = section.Body;
                    // streamedFileContent = 
                    //     await FileHelpers.ProcessStreamedFile(section, contentDisposition, 
                    //         ModelState, _permittedExtensions, _fileSizeLimit);


                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                }
                else if (MultipartRequestHelper
                         .HasFormDataContentDisposition(contentDisposition))
                {
                    // Don't limit the key name length because the 
                    // multipart headers length limit is already in effect.
                    var key = HeaderUtilities
                        .RemoveQuotes(contentDisposition!.Name).Value;
                    var encoding = GetEncoding(section);
                    if (encoding == null)
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }

                    using (var streamReader = new StreamReader(
                               section.Body,
                               encoding,
                               detectEncodingFromByteOrderMarks: true,
                               bufferSize: 1024,
                               leaveOpen: true))
                    {
                        // The value length limit is enforced by 
                        // MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);

                        if (formAccumulator.ValueCount >
                            DefaultFormOptions.ValueCountLimit)
                        {
                            // Form key count limit of 
                            // _defaultFormOptions.ValueCountLimit 
                            // is exceeded.
                            ModelState.AddModelError("File",
                                $"The request couldn't be processed (Error 3).");
                            // Log error

                            return BadRequest(ModelState);
                        }
                    }
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        // Bind form data to the model
        var formData = new FormData();
        var formValueProvider = new FormValueProvider(
            BindingSource.Form,
            new FormCollection(formAccumulator.GetResults()),
            CultureInfo.CurrentCulture);
        var bindingSuccessful = await TryUpdateModelAsync(formData, prefix: "",
            valueProvider: formValueProvider);

        if (!bindingSuccessful)
        {
            ModelState.AddModelError("File",
                "The request couldn't be processed (Error 5).");
            // Log error

            return BadRequest(ModelState);
        }

        // **WARNING!**
        // In the following example, the file is saved without
        // scanning the file's contents. In most production
        // scenarios, an anti-virus/anti-malware scanner API
        // is used on the file before making the file available
        // for download or for use by other systems. 
        // For more information, see the topic that accompanies 
        // this sample app.


        // var file = new AppFile()
        // {
        //     Content = streamedFileContent,
        //     UntrustedName = untrustedFileNameForStorage,
        //     Note = formData.Note,
        //     Size = streamedFileContent.Length, 
        //     UploadDT = DateTime.UtcNow
        // };
        //
        // _context.File.Add(file);
        // await _context.SaveChangesAsync();

        return Created(nameof(StreamingController), null);
    }

    #endregion

    #region snippet_UploadPhysical

    [HttpPost("upload")]
    [DisableFormValueModelBinding]
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(CancellationToken ct)
    {
        await _streamingUploader.HandleAsync("Store", ct);
        return Ok();
    }


    private async Task<IActionResult> UploadPhysical2()
    {
        if (Request.ContentType is null || !MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File",
                $"The request couldn't be processed (Error 1).");
            // Log error

            return BadRequest(ModelState);
        }

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            DefaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        var section = await reader.ReadNextSectionAsync();

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
                    ModelState.AddModelError("File",
                        $"The request couldn't be processed (Error 2).");
                    // Log error

                    return BadRequest(ModelState);
                }
                else
                {
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                        contentDisposition!.FileName.Value);
                    var trustedFileNameForFileStorage = Path.GetRandomFileName();

                    // **WARNING!**
                    // In the following example, the file is saved without
                    // scanning the file's contents. In most production
                    // scenarios, an anti-virus/anti-malware scanner API
                    // is used on the file before making the file available
                    // for download or for use by other systems. 
                    // For more information, see the topic that accompanies 
                    // this sample.

#if ORIGINAL_SAMPLE_CODE_USE_MEMSTREM
                    var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                        section, contentDisposition, ModelState, 
                        _permittedExtensions, _fileSizeLimit);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                    
                    using (var targetStream = System.IO.File.Create(
                               Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                    {
                        await targetStream.WriteAsync(streamedFileContent);

                        _logger.LogInformation(
                            "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                            "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                            trustedFileNameForDisplay, _targetFilePath,
                            trustedFileNameForFileStorage);
                    }
#endif
                    var bufferSize = 64 * 1024;
                    var totalBytes = 0L;
                    if (section.Headers is not null && section.Headers.TryGetValue(HeaderNames.ContentLength, out var contentLengthStringValue))
                    {
                        if (long.TryParse(contentLengthStringValue, out var contentLength))
                        {
                            totalBytes = contentLength;
                        }
                    }
                    var writtenBytes = 0;
                    var bytes = new byte[bufferSize];
                    var buffer = bytes.AsMemory();
                    var targetFilePath = Path.Combine(_targetFilePath, trustedFileNameForDisplay); //trustedFileNameForFileStorage);
                    long progress = 0;
                    _logger.LogInformation("Begin Uploading : Length={TotalBytes:N}", totalBytes);
                    await using var targetStream 
                        = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                    while (true)
                    {
                        var readBytes = await section.Body.ReadAsync(buffer);
                        if (readBytes <= 0)
                        {
                            break;
                        }

                        await targetStream.WriteAsync(buffer[..readBytes]);
                        writtenBytes += readBytes;
                        var currProgress = totalBytes > 0 ? writtenBytes * 10L / totalBytes : 0;
                        if (currProgress > progress)
                        {
                            _logger.LogInformation("  uploading : {Percent} %", currProgress * 10);
                        }

                        progress = currProgress;
                        // _logger.LogInformation("   wrote {TotalBytes}", writtenBytes);
                    }
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        return Created(nameof(StreamingController), null);
    }

    #endregion

    private static Encoding? GetEncoding(MultipartSection section)
    {
        // var hasMediaTypeHeader = 
        //     MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);
        //
        // // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
        // // most cases.  
        // if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType!.Encoding))
        // {
        //     return Encoding.UTF8;
        // }
        //
        // return mediaType.Encoding;
        Encoding? encoding = null;

        if (MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType))
        {
            encoding = mediaType.Encoding;
        }

        return encoding;
    }
}

public class FormData
{
    public string Note { get; set; }
}