using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace FileStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileStoreController : ControllerBase
    {
        private readonly IHostEnvironment _host;
        private readonly ILogger<FileStoreController> _logger;

        public FileStoreController(
            IHostEnvironment host, 
            ILogger<FileStoreController> logger
            )
        {
            _host = host;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAsync(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var filePath = Path.Combine(_host.ContentRootPath, "Store", fileName);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, contentType);
        }
        
        [HttpPost("Upload")]
        [RequestFormLimits(MultipartBodyLengthLimit = 100L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> PostAsync(List<IFormFile> files, CancellationToken ct)
        {
            var size = files.Sum(x => x.Length);
            var basePath = _host.ContentRootPath;
            var filePaths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    continue;
                }

                var filePath = Path.Combine(basePath, "Store", file.FileName);
                _logger.LogInformation("Receiving File : {FilePath}", filePath);
                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                await file.CopyToAsync(stream, ct);
                filePaths.Add(filePath);
            }

            return Ok(new
            {
                count = files.Count,
                size,
                filePaths
            });
        }
    }
}
