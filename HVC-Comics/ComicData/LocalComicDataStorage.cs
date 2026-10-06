namespace HVC_Comics.ComicData;

public sealed class LocalComicDataStorage : IComicDataStorage
{
    private readonly string _filePath;

    public LocalComicDataStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<Stream?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        return new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);
    }
}