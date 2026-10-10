using HVC_Comics.Repositories;

using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class ComicController(
IComicRepository repository) : Controller
{
    private readonly IComicRepository _repository = repository;

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var comic = await _repository.GetRandomAsync(cancellationToken);
        //var comic = await _repository.GetByIdAsync(2101);
        return View(comic);
    }
}
