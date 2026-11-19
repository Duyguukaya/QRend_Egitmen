using Microsoft.AspNetCore.Mvc;
using QRend_Egitmen.Models;
using QRend_Egitmen.Services;

namespace QRend_Egitmen.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApiService _api;

        public AttendanceController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? lessonId)
        {
            if (HttpContext.Session.GetString("IsLoggedIn") != "true")
                return RedirectToAction("Login", "Account");

            var lessons = await _api.GetMyLessonsAsync();
            var selectedId = lessonId ?? lessons.FirstOrDefault()?.Id.ToString();

            var legacyLessons = lessons.Select(l => new Lesson
            {
                Id = l.Id.ToString(),
                Code = l.Code ?? "",
                Name = l.Name,
                Schedule = l.Schedule ?? "",
                StudentCount = l.StudentCount,
                Students = new()
            }).ToList();

            int? selectedIdInt = null;
            if (int.TryParse(selectedId, out var parsed)) selectedIdInt = parsed;

            var activeSession = selectedIdInt.HasValue ? await _api.GetActiveSessionAsync(selectedIdInt) : null;

            // Map to legacy model
            AttendanceSession? legacySession = null;
            if (activeSession != null)
            {
                legacySession = new AttendanceSession
                {
                    LessonId = activeSession.LessonId.ToString(),
                    SessionId = activeSession.Id.ToString(),
                    StartedAt = activeSession.StartedAt.ToString("HH:mm:ss"),
                    IsActive = true,
                    JoinedStudents = activeSession.JoinedStudents.Select(s => new Student
                    {
                        Id = s.StudentId.ToString(),
                        Name = s.StudentName,
                        StudentNo = s.StudentNo,
                        JoinedAt = s.JoinedAt.ToString("HH:mm:ss")
                    }).ToList()
                };
            }

            // Attendance history
            var history = selectedIdInt.HasValue
                ? await _api.GetAttendanceHistoryAsync(selectedIdInt)
                : new List<AttendanceHistoryItem>();

            var legacyHistory = history.Select(h => new AttendanceRecord
            {
                LessonId = "",
                LessonName = h.LessonName,
                LessonCode = h.LessonCode,
                SessionId = h.SessionId.ToString(),
                Date = h.Date.ToString("dd.MM.yyyy"),
                StartedAt = h.StartedAt,
                EndedAt = h.EndedAt,
                JoinedStudents = h.JoinedStudents.Select(s => new Student
                {
                    Id = s.StudentId.ToString(),
                    Name = s.StudentName,
                    StudentNo = s.StudentNo,
                    JoinedAt = s.JoinedAt.ToString("HH:mm:ss")
                }).ToList()
            }).ToList();

            ViewBag.Lessons = legacyLessons;
            ViewBag.SelectedLessonId = selectedId;
            ViewBag.ActiveSession = legacySession;
            ViewBag.AttendanceHistory = legacyHistory;

            // QR token for display
            ViewBag.SessionToken = activeSession?.SessionToken;
            ViewBag.ApiSessionId = activeSession?.Id;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Start(string lessonId)
        {
            if (int.TryParse(lessonId, out var id))
            {
                await _api.StartAttendanceAsync(id);
            }
            return RedirectToAction("Index", new { lessonId });
        }

        [HttpPost]
        public async Task<IActionResult> Stop()
        {
            var apiSessionId = Request.Form["apiSessionId"].FirstOrDefault();
            var lessonId = Request.Form["lessonId"].FirstOrDefault();

            if (int.TryParse(apiSessionId, out var sessionId))
            {
                await _api.StopAttendanceAsync(sessionId);
            }
            return RedirectToAction("Index", new { lessonId });
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(string studentId, string? manualName, string? manualNo)
        {
            var apiSessionId = Request.Form["apiSessionId"].FirstOrDefault();
            var lessonId = Request.Form["lessonId"].FirstOrDefault();

            if (int.TryParse(apiSessionId, out var sessionId) && int.TryParse(studentId, out var sId))
            {
                await _api.AddStudentToSessionAsync(sessionId, sId);
            }

            return RedirectToAction("Index", new { lessonId });
        }
    }
}
