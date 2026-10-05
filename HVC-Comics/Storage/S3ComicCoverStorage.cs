using Amazon.S3;
using Amazon.S3.Model;

namespace HVC_Comics.Storage;

public sealed class S3ComicCoverStorage : IComicCoverStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _prefix;

    public S3ComicCoverStorage(
        IAmazonS3 s3,
        IConfiguration configuration)
    {
        _s3 = s3;

        _bucket =
            configuration["ComicCovers:S3:Bucket"]
            ?? throw new InvalidOperationException(
                "ComicCovers:S3:Bucket não configurado.");

        _prefix =
            configuration["ComicCovers:S3:Prefix"]
            ?? "Comics/Covers/Current/";
    }

    public async Task<Stream?> GetAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        path = path.TrimStart('/');

        var key = $"{_prefix.TrimEnd('/')}/{path}";

        try
        {
            var response = await _s3.GetObjectAsync(
                new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = key
                },
                cancellationToken);

            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex)
            when (ex.StatusCode ==
                  System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
