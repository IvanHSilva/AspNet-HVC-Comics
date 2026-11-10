using HVC_Comics.Repositories;
using HVC_Comics.Views.Comic;

using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class ComicController(IComicRepository repository) : Controller
{
    private readonly IComicRepository _repository = repository;

    [HttpGet("")]
    [HttpGet("edition")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var comic = await _repository.GetRandomAsync(cancellationToken);

        if (comic is null) return NotFound();

        var slug = ComicSlug.Create(comic.Name, comic.Number);

        return RedirectToAction(nameof(Edition), new { slug });
    }

    [HttpGet("edition/{slug}")]
    public async Task<IActionResult> Edition(string slug, CancellationToken cancellationToken)
    {
        var comic = await _repository.GetBySlugAsync(slug, cancellationToken);

        if (comic is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        var canonicalSlug = ComicSlug.Create(comic.Name, comic.Number);

        if (!string.Equals(slug, canonicalSlug, StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Edition), new { slug = canonicalSlug });

        var baseName = ComicVolume.GetBaseName(comic.Name, comic.Volume);
        var editions = await _repository.GetEditionsAsync(baseName, comic.Volume, cancellationToken);
        var volumeStarts = await _repository.GetVolumeStartsAsync(baseName, cancellationToken);

        ViewData["Editions"] = editions;
        ViewData["VolumeStarts"] = volumeStarts;
        ViewData["CurrentVolume"] = comic.Volume;
        ViewData["CollectionName"] = baseName;

        ViewData["GalleryUrl"] = Url.Action(nameof(Gallery), new
        {
            gallerySlug = ComicSlug.CreateGallery(comic.Name, comic.Volume)
        });

        ViewData["Volumes"] = editions.Select(c => c.Volume).Distinct().Order().ToList();
        ViewData["CurrentVolume"] = comic.Volume;
        ViewData["CollectionSlug"] = ComicSlug.Normalize(comic.Name);

        return View("Index", comic);
    }

    [HttpGet("gallery/{gallerySlug}")]
    public async Task<IActionResult> Gallery(string gallerySlug,
    CancellationToken cancellationToken)
    {

        var hasVolume = ComicSlug.TryParseGallery(gallerySlug,
            out var collectionSlug, out var requestedVolume);
        var volumeStarts = await _repository.GetVolumeStartsAsync(
            collectionSlug, cancellationToken);

        if (volumeStarts.Count == 0)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        var volumes = volumeStarts.Select(c => c.Volume).Distinct().Order().ToList();
        var currentVolume = hasVolume ? requestedVolume : 1;

        if (!volumes.Contains(currentVolume))
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        var editions = await _repository.GetEditionsAsync(
            collectionSlug, currentVolume, cancellationToken);

        if (editions.Count == 0)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        editions = [.. editions.OrderBy(c => c.Number).ThenBy(c => c.Id)];

        ViewData["CollectionName"] = ComicVolume.GetBaseName(editions[0].Name, editions[0].Volume);
        ViewData["CollectionSlug"] = collectionSlug;
        ViewData["Volumes"] = volumes;
        ViewData["CurrentVolume"] = currentVolume;

        return View("Gallery", editions);
    }
}
