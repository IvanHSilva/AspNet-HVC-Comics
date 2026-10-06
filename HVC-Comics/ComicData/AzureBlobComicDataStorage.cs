using Azure.Identity;
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

        if (string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException(
                "O nome do blob não pode ser vazio.",
                nameof(blobName));
        }

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

        _blobName = blobName.TrimStart('/');
    }

    public async Task<Stream?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var blob =
            _container.GetBlobClient(_blobName);

        try
        {
            var response =
                await blob.DownloadStreamingAsync(
                    cancellationToken: cancellationToken);

            return response.Value.Content;
        }
        catch (Azure.RequestFailedException ex)
            when (ex.Status == 404)
        {
            return null;
        }
    }
}
