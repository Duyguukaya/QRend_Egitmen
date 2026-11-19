namespace QRend_Egitmen.Models
{
    public class AttendanceRecord
    {
        public string LessonId { get; set; } = "";
        public string LessonName { get; set; } = "";
        public string LessonCode { get; set; } = "";
        public string SessionId { get; set; } = "";
        public string Date { get; set; } = "";
        public string StartedAt { get; set; } = "";
        public string EndedAt { get; set; } = "";
        public List<Student> JoinedStudents { get; set; } = new();
    }
}
