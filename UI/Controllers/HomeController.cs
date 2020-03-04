using Microsoft.AspNetCore.Mvc;

namespace DiceGame.Akka.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
