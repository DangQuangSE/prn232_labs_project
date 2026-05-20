namespace PRN232.LMSSystem.Services.Models.Request;

public class EnrollmentRequest
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = "Enrolled";
}
