using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public List<LoginModel> PutValue()
        {
            var users = new List<LoginModel>
            {
                new LoginModel { id = 1, username = "admin", password = "12345" },
                new LoginModel { id = 2, username = "mycodingproject", password = "myc546" },
                new LoginModel { id = 3, username = "my", password = "myc" }
            };
            return users;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(LoginModel usr)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", usr);
            }

            var u = PutValue();
            var ue = u.Where(x => x.username == usr.username);
            var up = ue.Where(p => p.password == usr.password);

            if (up.Count() == 1)
            {
                TempData["message"] = "Login Success";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.message = "Login Failed";
                return View("Index", usr);
            }
        }
    }
}
