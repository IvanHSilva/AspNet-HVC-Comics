using Azure.Storage.Blobs;

namespace HVC_Comics.ComicData;

public sealed class AzureBlobComicDataStorage : IComicDataStorage
{
    private readonly BlobContainerClient _container;
    private readonly string _blobName;

    public AzureBlobComicDataStorage(
        string accountName,
        string containerName,
        string blobName)
    {
        var serviceUri =
            new Uri(
                $"https://{accountName}.blob.core.windows.net");

        _container =
            new BlobContainerClient(
                serviceUri,
                new Azure.Identity.DefaultAzureCredential());

        _blobName = blobName;
    }

    public async Task<Stream?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var blob =
            _container.GetBlobClient(_blobName);

        if (!await blob.ExistsAsync(cancellationToken))
        {
            return null;
        }

        var response =
            await blob.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

        return response.Value.Content;
    }
}