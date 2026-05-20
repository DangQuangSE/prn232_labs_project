namespace PRN232.LMSSystem.Services.Models.Request;

public class CourseRequest
{
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
}
