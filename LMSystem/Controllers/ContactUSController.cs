using Microsoft.AspNetCore.Mvc;

namespace LMSystem.Controllers
{
    public class ContactUSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
