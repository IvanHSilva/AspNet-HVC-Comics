using HVC_Comics.Repositories;
using HVC_Comics.Views.Comic;

using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class ComicController(
    IComicRepository repository) : Controller
{
    private readonly IComicRepository _repository = repository;

    [HttpGet("")]
    [HttpGet("edition")]
    // Sorted edition
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var comic = await _repository.GetRandomAsync(cancellationToken);
        //var comic = await _repository.GetByIdAsync(1610);

        if (comic is null) return NotFound();

        var slug = ComicSlug.Create(comic.Name, comic.Number);

        return RedirectToAction(nameof(Edition), new { slug });
    }

    // Edition number
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

        var editions = await _repository.GetEditionsAsync(comic.Name, cancellationToken);

        ViewData["Editions"] = editions;
        ViewData["GalleryUrl"] = Url.Action(nameof(Gallery), new
        {
            collectionSlug = ComicSlug.Normalize(comic.Name)
        });

        return View("Index", comic);
    }

    [HttpGet("edition/{collectionSlug}/galeria")]
    public async Task<IActionResult> Gallery(string collectionSlug,
        CancellationToken cancellationToken)
    {
        var editions = await _repository.GetEditionsAsync(collectionSlug, cancellationToken);

        if (editions.Count == 0)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        if (editions.Count == 1)
        {
            var slug = ComicSlug.Create(editions[0].Name, editions[0].Number);
            return RedirectToAction(nameof(Edition), new { slug });
        }
        ViewData["CollectionName"] = editions[0].Name;

        return View("Gallery", editions);
    }
}
