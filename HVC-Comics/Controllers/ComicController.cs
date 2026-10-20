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

        return View("Index", comic);
    }
}
