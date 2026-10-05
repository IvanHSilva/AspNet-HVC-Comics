using HVC_Comics.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class HomeController(
    JsonComicRepository repository) : Controller
{
    private readonly JsonComicRepository _repository = repository;

    public IActionResult Index()
    {
        var comic = _repository.GetRandom();
        //var comic = _repository.GetById(1);

        return View(comic);
    }
}
