namespace PRN232.LMSSystem.Services.Models.Response;

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public int EnrollmentCount { get; set; }
}
