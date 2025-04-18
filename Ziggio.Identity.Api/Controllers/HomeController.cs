using Microsoft.AspNetCore.Mvc;

namespace Ziggio.Identity.Api.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}