namespace HVC_Comics.Storage;

public interface IComicCoverStorage
{
    Task<Stream?> GetAsync(
        string path,
        CancellationToken cancellationToken = default);
}
