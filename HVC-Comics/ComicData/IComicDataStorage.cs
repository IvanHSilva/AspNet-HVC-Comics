namespace HVC_Comics.ComicData;

public interface IComicDataStorage
{
    Task<Stream?> GetAsync(
    CancellationToken cancellationToken = default);
}