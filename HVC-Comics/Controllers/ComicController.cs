using Microsoft.AspNetCore.Mvc;
using HVC_Comics.Repositories;

namespace HVC_Comics.Controllers;

public class ComicController(ComicRepository repository) : Controller
{
    private readonly ComicRepository _repository = repository;

    public IActionResult Index(int page = 1)
    {
        var comics = _repository.GetPaged(page, 50);

        ViewBag.CurrentPage = page;

        return View(comics);
    }
}
