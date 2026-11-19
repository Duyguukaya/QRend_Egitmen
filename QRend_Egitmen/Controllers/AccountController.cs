using Microsoft.AspNetCore.Mvc;
using QRend_Egitmen.Services;

namespace QRend_Egitmen.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api) => _api = api;

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Index", "Lessons");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _api.LoginAsync(email, password);
            if (result == null || result.Role != "Instructor")
            {
                ViewBag.Error = "E-posta veya şifre hatalı, ya da eğitmen yetkisi yok.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("IsLoggedIn", "true");
            HttpContext.Session.SetString("UserName", result.FullName);
            HttpContext.Session.SetString("UserEmail", result.Email);

            return RedirectToAction("Index", "Lessons");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
