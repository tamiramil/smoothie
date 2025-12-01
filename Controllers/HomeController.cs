using Microsoft.AspNetCore.Mvc;

namespace smoothie.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() {
        return View();
    }
}