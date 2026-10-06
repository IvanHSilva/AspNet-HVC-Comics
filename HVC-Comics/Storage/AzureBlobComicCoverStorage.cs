using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace HVC_Comics.Storage;

public sealed class AzureBlobComicCoverStorage : IComicCoverStorage
{
    private readonly BlobContainerClient _container;
    private readonly string _prefix;

    public AzureBlobComicCoverStorage(
        string accountName,
        string containerName,
        string prefix)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new ArgumentException(
                "O nome da Storage Account não pode ser vazio.",
                nameof(accountName));
        }

        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentException(
                "O nome do container não pode ser vazio.",
                nameof(containerName));
        }

        _prefix = prefix?.Trim('/') ?? "";

        var serviceUri =
            new Uri(
                $"https://{accountName}.blob.core.windows.net");

        var serviceClient =
            new BlobServiceClient(
                serviceUri,
                new DefaultAzureCredential());

        _container =
            serviceClient.GetBlobContainerClient(
                containerName);
    }

    public async Task<Stream?> GetAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var blobName = BuildBlobName(relativePath);

        var blobClient =
            _container.GetBlobClient(blobName);

        try
        {
            var response =
                await blobClient.DownloadStreamingAsync(
                    cancellationToken: cancellationToken);

            return response.Value.Content;
        }
        catch (RequestFailedException ex)
            when (ex.Status == 404)
        {
            return null;
        }
    }

    private string BuildBlobName(string relativePath)
    {
        var path =
            relativePath
                .Replace('\\', '/')
                .Trim('/');

        if (string.IsNullOrWhiteSpace(_prefix))
            return path;

        return $"{_prefix}/{path}";
    }


}
