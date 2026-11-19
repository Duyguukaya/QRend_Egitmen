using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QRend_Egitmen.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _baseUrl;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IConfiguration config, IHttpContextAccessor httpContext)
        {
            _http = http;
            _httpContext = httpContext;
            _baseUrl = config["ApiBaseUrl"] ?? "http://localhost:5050";
        }

        private void SetAuth()
        {
            var token = _httpContext.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // === AUTH ===
        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var body = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_baseUrl}/api/auth/login", content);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
        }

        // === LESSONS (Instructor) ===
        public async Task<List<LessonItem>> GetMyLessonsAsync()
        {
            SetAuth();
            var response = await _http.GetAsync($"{_baseUrl}/api/lessons/my");
            if (!response.IsSuccessStatusCode) return new();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LessonItem>>(json, _jsonOptions) ?? new();
        }

        // === ATTENDANCE ===
        public async Task<AttendanceSessionData?> StartAttendanceAsync(int lessonId)
        {
            SetAuth();
            var body = JsonSerializer.Serialize(new { lessonId });
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_baseUrl}/api/attendance/start", content);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AttendanceSessionData>(json, _jsonOptions);
        }

        public async Task<bool> StopAttendanceAsync(int sessionId)
        {
            SetAuth();
            var response = await _http.PostAsync($"{_baseUrl}/api/attendance/stop/{sessionId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<AttendanceSessionData?> GetActiveSessionAsync(int? lessonId = null)
        {
            SetAuth();
            var url = $"{_baseUrl}/api/attendance/active";
            if (lessonId.HasValue) url += $"?lessonId={lessonId}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json) || json == "null") return null;
            return JsonSerializer.Deserialize<AttendanceSessionData>(json, _jsonOptions);
        }

        public async Task<List<AttendanceHistoryItem>> GetAttendanceHistoryAsync(int? lessonId = null)
        {
            SetAuth();
            var url = $"{_baseUrl}/api/attendance/history";
            if (lessonId.HasValue) url += $"?lessonId={lessonId}";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AttendanceHistoryItem>>(json, _jsonOptions) ?? new();
        }

        public async Task<List<StudentItem>> GetLessonStudentsAsync(int lessonId)
        {
            SetAuth();
            var response = await _http.GetAsync($"{_baseUrl}/api/attendance/lesson-students/{lessonId}");
            if (!response.IsSuccessStatusCode) return new();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<StudentItem>>(json, _jsonOptions) ?? new();
        }

        public async Task<bool> AddStudentToSessionAsync(int sessionId, int studentId)
        {
            SetAuth();
            var response = await _http.PostAsync($"{_baseUrl}/api/attendance/add-student?sessionId={sessionId}&studentId={studentId}", null);
            return response.IsSuccessStatusCode;
        }
    }

    // Response models
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class LessonItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string? Schedule { get; set; }
        public int StudentCount { get; set; }
    }

    public class AttendanceSessionData
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string LessonName { get; set; } = "";
        public string LessonCode { get; set; } = "";
        public string SessionToken { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive { get; set; }
        public List<AttendanceStudentData> JoinedStudents { get; set; } = new();
    }

    public class AttendanceStudentData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public DateTime JoinedAt { get; set; }
    }

    public class AttendanceHistoryItem
    {
        public int SessionId { get; set; }
        public string LessonName { get; set; } = "";
        public string LessonCode { get; set; } = "";
        public DateTime Date { get; set; }
        public string StartedAt { get; set; } = "";
        public string EndedAt { get; set; } = "";
        public int JoinedCount { get; set; }
        public int TotalStudents { get; set; }
        public List<AttendanceStudentData> JoinedStudents { get; set; } = new();
    }

    public class StudentItem
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
