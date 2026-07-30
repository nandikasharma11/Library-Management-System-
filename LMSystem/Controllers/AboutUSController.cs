using Microsoft.AspNetCore.Mvc;

namespace LMSystem.Controllers
{
    public class AboutUSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
