namespace FileStore.Lib.Contracts;

public class BucketList
{
    public List<string> BucketNames { get; set; } = new();
    public static BucketList Empty { get; set; } = new();

    public override string ToString()
    {
        var bucketNames = string.Join(", ", BucketNames);
        return $"{nameof(BucketNames)}: {bucketNames}";
    }
}