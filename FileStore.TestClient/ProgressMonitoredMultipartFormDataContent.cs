using System.Diagnostics.Contracts;
using System.Net;

namespace FileStore.TestClient;

class ProgressableStreamContent : HttpContent
{
    private const int DefaultBufferSize = 4096;

    private readonly Stream _content;
    private readonly int _bufferSize;
    private bool _contentConsumed;
    private readonly IProgress<UploadProgress>? _progress;

    public ProgressableStreamContent(Stream content, int bufferSize = DefaultBufferSize, IProgress<UploadProgress>? progress = null)
    {
        if(bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _bufferSize = bufferSize;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        Contract.Assert(stream != null);

        PrepareContent();

        var buffer = new byte[_bufferSize];
        var size = _content.Length;
        var uploaded = 0;

        await using (_content)
        {
            while (true)
            {
                var readBytes = await _content.ReadAsync(buffer, 0, _bufferSize);
                if (readBytes <= 0)
                {
                    break;
                }

                await stream.WriteAsync(buffer, 0, readBytes);
                uploaded += readBytes;
                this._progress?.Report(new UploadProgress(uploaded, size));
            }
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _content.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            _content.Dispose();
        }
        base.Dispose(disposing);
    }
    
    private void PrepareContent()
    {
        if(_contentConsumed)
        {
            // If the content needs to be written to a target stream a 2nd time, then the stream must support
            // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
            // stream (e.g. a NetworkStream).
            if(_content.CanSeek)
            {
                _content.Position = 0;
            }
            else
            {
                throw new InvalidOperationException("SR.net_http_content_stream_already_read");
            }
        }

        _contentConsumed = true;
    }
}

public class UploadProgress
{
    public UploadProgress(long bytesTransfered, long? totalBytes = null)
    {
        BytesTransfered = bytesTransfered;
        TotalBytes = totalBytes;
        if (totalBytes.HasValue)
        {
            ProgressPercentage = (int)((float)bytesTransfered / totalBytes.Value * 100);
        }
    }
    public long BytesTransfered { get; }

    public int ProgressPercentage { get; }

    public long? TotalBytes { get; }

    public override string ToString()
    {
        return $"{nameof(BytesTransfered)}: {BytesTransfered}, {nameof(ProgressPercentage)}: {ProgressPercentage}, {nameof(TotalBytes)}: {TotalBytes}";
    }
}
