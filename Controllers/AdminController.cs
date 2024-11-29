using Microsoft.AspNetCore.Mvc;
using RestaurangWebAPI.Models;

namespace RestaurangWebAPI.Controllers
{
    public class AdminController : Controller
    {

                private const string AdminUsername = "admin";
                private const string AdminPassword = "password123";

                public IActionResult Login()
                {
                    return View();
                }

        [HttpPost]
        public IActionResult Login(AdminLogin model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == AdminUsername && model.Password == AdminPassword)
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Invalid username or password.";
            }

            return View(model);
        }


        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Index", "Home");
        }

    }
}
