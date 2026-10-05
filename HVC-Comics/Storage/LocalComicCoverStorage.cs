namespace HVC_Comics.Storage;

public sealed class LocalComicCoverStorage : IComicCoverStorage
{
    private readonly string _rootPath;

    public LocalComicCoverStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException(
                "O caminho das capas não pode ser vazio.",
                nameof(rootPath));

        _rootPath = Path.GetFullPath(rootPath);
    }

    public Task<Stream?> GetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<Stream?>(null);

        path = path.TrimStart('/', '\\');

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, path));

        // Impede acesso a arquivos fora da pasta configurada.
        if (!fullPath.StartsWith(
                _rootPath.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<Stream?>(null);
        }

        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }


}