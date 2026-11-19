namespace QRend_Egitmen.Models
{
    public class AttendanceSession
    {
        public string LessonId { get; set; } = "";
        public string SessionId { get; set; } = "";
        public string StartedAt { get; set; } = "";
        public List<Student> JoinedStudents { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
