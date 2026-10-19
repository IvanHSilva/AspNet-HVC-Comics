using Microsoft.AspNetCore.Mvc;

namespace HVC_Comics.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Comic");
}
