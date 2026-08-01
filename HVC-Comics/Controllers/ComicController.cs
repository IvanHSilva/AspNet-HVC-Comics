using HVC_Comics.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class ComicController(ComicRepository repository) : Controller
{
    private readonly ComicRepository _repository = repository;

    public IActionResult Index()
    {
        var comics = _repository.GetAll();

        return View(comics);
    }
}
