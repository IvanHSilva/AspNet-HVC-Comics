using HVC_Comics.Storage;

using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

[Route("capas")]
public class CoversController : Controller
{
    private readonly IComicCoverStorage _storage;

    public CoversController(IComicCoverStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("{**path}")]
    public async Task<IActionResult> Get(
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
            return NotFound();

        var stream = await _storage.GetAsync(
            path,
            cancellationToken);

        if (stream is null)
            return NotFound();

        return File(
            stream,
            GetContentType(path));
    }

    private static string GetContentType(string path)
    {
        var extension =
            Path.GetExtension(path)
                .ToLowerInvariant();

        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
