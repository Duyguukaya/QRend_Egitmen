using Microsoft.AspNetCore.Mvc;
using QRend_Egitmen.Services;

namespace QRend_Egitmen.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ApiService _api;

        public LessonsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login", "Account");

            var lessons = await _api.GetMyLessonsAsync();

            var legacyLessons = lessons.Select(l => new Models.Lesson
            {
                Id = l.Id.ToString(),
                Code = l.Code ?? "",
                Name = l.Name,
                Schedule = l.Schedule ?? "",
                StudentCount = l.StudentCount,
                Students = new()
            }).ToList();

            return View(legacyLessons);
        }
    }
}
