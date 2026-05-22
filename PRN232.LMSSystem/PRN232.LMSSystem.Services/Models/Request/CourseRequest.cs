namespace PRN232.LMSSystem.Services.Models.Request;

/// <summary>
/// Request body for creating or updating a Course record.
/// </summary>
public class CourseRequest
{
    /// <summary>Name/code of the course. Format: SubjectCode_ClassCode_SemesterSuffix.</summary>
    /// <example>PRN232_SE1701_S1</example>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>ID of the semester this course belongs to. Must reference an existing Semester.</summary>
    /// <example>1</example>
    public int SemesterId { get; set; }
}
