namespace FileStore.Lib.Uploader;

public interface IProgressEventNotifier
{
    Task NotifyAsync(string id, float progress);
}

public class DummyProgressEventNotifier : IProgressEventNotifier
{
    public Task NotifyAsync(string id, float progress)
    {
        // no op
        return Task.CompletedTask;
    }
}