using Microsoft.AspNetCore.Mvc;
using HVC_Comics.Repositories;

namespace HVC_Comics.Controllers;

public class ComicController(ComicRepository repository) : Controller
{
    private readonly ComicRepository _repository = repository;

    public IActionResult Index(int page = 1)
    {
        var userAgent = Request.Headers.UserAgent.ToString();

        int pageSize = 50;

        if (userAgent.Contains("Mobile"))
        {
            pageSize = 10;
        }

        var result = _repository.GetPaged(page, pageSize);

        return View(result);
    }
}
