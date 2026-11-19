namespace QRend_Egitmen.Models
{
    public class Lesson
    {
        public string Id { get; set; } = "";
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Schedule { get; set; } = "";
        public int StudentCount { get; set; }
        public List<Student> Students { get; set; } = new();
    }
}
