namespace PRN232.LMSSystem.Services.Models.Response;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public StudentResponse? Student { get; set; }

    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public CourseResponse? Course { get; set; }

    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
