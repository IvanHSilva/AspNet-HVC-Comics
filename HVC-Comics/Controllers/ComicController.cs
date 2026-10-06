using HVC_Comics.Repositories;

using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class ComicController(
IComicRepository repository) : Controller
{
    private readonly IComicRepository _repository = repository;

    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _repository.GetPagedAsync(
                page,
                pageSize,
                cancellationToken);

        return View(result);
    }
}