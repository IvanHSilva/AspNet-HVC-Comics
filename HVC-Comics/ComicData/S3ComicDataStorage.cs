using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace HVC_Comics.ComicData;

public sealed class S3ComicDataStorage(
    IAmazonS3 s3,
    string bucket,
    string key) : IComicDataStorage
{
    private readonly IAmazonS3 _s3 = s3;
    private readonly string _bucket = bucket;
    private readonly string _key = key;

    public async Task<Stream?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3.GetObjectAsync(
                new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = _key
                },
                cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
